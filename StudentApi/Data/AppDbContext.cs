using StudentApi.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentApi.Data;

public class AppDbContext : DbContext
{
	public DbSet<Student> Students => Set<Student>();
	public DbSet<User> Users => Set<User>();

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}
}
