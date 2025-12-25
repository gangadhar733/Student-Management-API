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
		public async Task<Student> AddAsync(Student student, CancellationToken cancellationToken)
		{
			_context.Students.Add(student);
			await _context.SaveChangesAsync(cancellationToken);
			return student;
		}

		public async Task DeleteAsync(int id, CancellationToken cancellationToken)
		{
			var student = await _context.Students.FindAsync(id);
			if (student is null)
				return;

			_context.Students.Remove(student);
			await _context.SaveChangesAsync(cancellationToken);
		}

		public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken)
		{
			return await _context.Students.ToListAsync(cancellationToken);
		}

		public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken)
		{
			return await _context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
		}

		public async Task UpdateAsync(Student student, CancellationToken cancellationToken)
		{
			_context.Students.Update(student);
			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}
