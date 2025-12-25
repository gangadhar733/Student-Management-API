using Microsoft.EntityFrameworkCore;
using StudentApi.Data;
using StudentApi.Repositories;
using StudentApi.Services;
using StudentApi.Models;
using StudentApi.Dtos;
using StudentApi.Mapping;
using StudentApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.MapGet("/students", async (IStudentService service, CancellationToken cancellationToken) =>
{
	var students = await service.GetStudents(cancellationToken);
	var result = students.Select(StudentMapping.ToDto); // Map the enity to Dto before returning the result
	return Results.Ok(result);
});

app.MapGet("/students/{id:int}", async (int id, IStudentService service, CancellationToken cancellationToken) =>
{
	var student = await service.GetStudent(id, cancellationToken);
	var result = student?.ToDto();
	return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/students", async (CreateUpdateStudentDto studentDto,
								IStudentService service,
								CancellationToken cancellationToken) =>
{
	if (!await service.ValidateAsync(studentDto))
		return Results.BadRequest("Invalid student data");


	var student = studentDto.ToEntity();

	var created = await service.CreateStudent(student, cancellationToken);

	return Results.Created($"/students/{created.Id}", created.ToDto());
});

app.MapDelete("/students/{id:int}", async (int id, IStudentService service, CancellationToken cancellationToken) =>
{
	var deleted = await service.DeleteStudent(id, cancellationToken);

	return deleted ? Results.Ok() : Results.NotFound();
});

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
});

app.Run();