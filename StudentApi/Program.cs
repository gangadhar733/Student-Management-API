using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StudentApi.Data;
using StudentApi.Dtos;
using StudentApi.Mapping;
using StudentApi.Middleware;
using StudentApi.Models;
using StudentApi.Repositories;
using StudentApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		In = ParameterLocation.Header,
		Description = "Enter JWT token with Bearer prefix. Example: Bearer YOUR_TOKEN_HERE",
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT"
	});

	options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecuritySchemeReference("Bearer", document),
			new List<string>()
		}
	});
});


var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_STUDENTAPI")
								?? jwt["Key"]
								?? throw new Exception("JWT secret key not configured");


var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateIssuerSigningKey = true,
		ValidateLifetime = true,

		ValidIssuer = jwt["Issuer"],
		ValidAudience = jwt["Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
	// Global rule: 100 requests per minute per IP
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 100,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0,
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst
			}));

	// Named rule for login (stricter)
	options.AddPolicy("LoginPolicy", context =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			factory: _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 5,
				Window = TimeSpan.FromMinutes(1)
			}));
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
		options.DocumentTitle = "My API Explorer"; // Optional title customization
	});
}
app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
	options.DocumentTitle = "My API Explorer"; // Optional title customization
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

//app.UseHttpsRedirection();

app.MapGet("/students", async (IStudentService service, CancellationToken cancellationToken) =>
{
	var students = await service.GetStudents(cancellationToken);
	var result = students.Select(StudentMapping.ToDto); // Map the enity to Dto before returning the result
	return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/students/{id:int}", async (int id, IStudentService service, CancellationToken cancellationToken) =>
{
	var student = await service.GetStudent(id, cancellationToken);
	var result = student?.ToDto();
	return result is null ? Results.NotFound() : Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/students", async (CreateUpdateStudentDto studentDto,
								IStudentService service,
								CancellationToken cancellationToken) =>
{
	if (!await service.ValidateAsync(studentDto))
		return Results.BadRequest("Invalid student data");


	var student = studentDto.ToEntity();

	var created = await service.CreateStudent(student, cancellationToken);

	return Results.Created($"/students/{created.Id}", created.ToDto());
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/students/{id:int}", async (int id, IStudentService service, CancellationToken cancellationToken) =>
{
	var deleted = await service.DeleteStudent(id, cancellationToken);

	return deleted ? Results.Ok() : Results.NotFound();
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPut("/students/{id:int}", async (int id, CreateUpdateStudentDto studentDto,
												IStudentService service,
												CancellationToken cancellationToken) =>
{
	if (!await service.ValidateAsync(studentDto))
		return Results.BadRequest("Invalid student data");
	var existing = await service.GetStudent(id, cancellationToken);

	if (existing is null)
	{
		return Results.NotFound("Invalid update operation: User does not exist");
	}

	existing.Name = studentDto.Name;
	existing.Email = studentDto.Email;
	existing.Age = studentDto.Age;

	await service.UpdateStudent(existing, cancellationToken);

	return Results.Ok(existing.ToDto());
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/login", async (IUserService userService, string username, string password, CancellationToken cancellationToken) =>
{
	var user = await userService.GetUserByUsername(username, cancellationToken);

	if (user is null)
		return Results.Unauthorized();

	var hasher = new PasswordHasher<User>();
	var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);

	if (result == PasswordVerificationResult.Failed)
		return Results.Unauthorized();

	var jwt = app.Configuration.GetSection("Jwt");
	var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_STUDENTAPI")
								?? jwt["Key"]
								?? throw new Exception("JWT secret key not configured");
	var key = Encoding.UTF8.GetBytes(secretKey);

	var claims = new[]
	{
		new Claim(ClaimTypes.Name, username),
		new Claim(ClaimTypes.Role, user.Role)
	};

	var token = new JwtSecurityToken(
		issuer: jwt["Issuer"],
		audience: jwt["Audience"],
		claims: claims,
		expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwt["ExpiresInMinutes"])),
		signingCredentials: new SigningCredentials(
					new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
		);

	var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

	return Results.Ok(new
	{
		token = tokenString,
		role = user.Role,
		status = "Login Success"
	});
}).RequireRateLimiting("LoginPolicy");

app.MapPost("/register", async (IUserService userService, string username, string password, CancellationToken cancellationToken) =>
{
	if (await userService.IsExistingUser(username, cancellationToken))
		return Results.BadRequest("User already exists");

	var hasher = new PasswordHasher<User>();
	var user = new User
	{
		Username = username,
	};
	user.Role = (username == "administrator") ? "Admin" : "User";
	user.PasswordHash = hasher.HashPassword(user, password);

	await userService.CreateUser(user, cancellationToken);

	return Results.Ok("User registered successfully");
});

app.Run();