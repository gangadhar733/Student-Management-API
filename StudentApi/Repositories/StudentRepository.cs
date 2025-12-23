using Microsoft.EntityFrameworkCore;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Repositories
{
	public class StudentRepository : IStudentRepository
	{
		private readonly AppDbContext _context;
		public StudentRepository(AppDbContext context)
		{
			_context = context;
		}
		public async Task<Student> AddAsync(Student student)
		{
			_context.Students.Add(student);
			await _context.SaveChangesAsync();
			return student;
		}

		public async Task DeleteAsync(int id)
		{
			var student = await _context.Students.FindAsync(id);
			if (student is null)
				return;

			_context.Students.Remove(student);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<Student>> GetAllAsync()
		{
			return await _context.Students.ToListAsync();
		}

		public async Task<Student?> GetByIdAsync(int id)
		{
			return await _context.Students.FindAsync(id);
		}

		public async Task UpdateAsync(Student student)
		{
			_context.Students.Update(student);
			await _context.SaveChangesAsync();
		}
	}
}
