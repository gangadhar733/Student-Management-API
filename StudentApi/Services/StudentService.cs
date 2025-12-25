using StudentApi.Models;
using StudentApi.Repositories;

namespace StudentApi.Services
{
	public class StudentService : IStudentService
	{
		private readonly IStudentRepository _repo;
		private readonly ILogger<StudentService> _logger;
		public StudentService(IStudentRepository repo, ILogger<StudentService> logger)
		{
			_repo = repo;
			_logger = logger;
		}

		public Task<Student> CreateStudent(Student student, CancellationToken cancellationToken)
		{
			return _repo.AddAsync(student, cancellationToken);
		}

		public async Task<bool> DeleteStudent(int id, CancellationToken cancellationToken)
		{
			var student = await _repo.GetByIdAsync(id, cancellationToken);

			if (student is null)
				return false;

			await _repo.DeleteAsync(id, cancellationToken);
			_logger.LogInformation($"Deleted student with email: {student.Email}");
			return true;
		}

		public Task<Student?> GetStudent(int id, CancellationToken cancellationToken)
		{
			return _repo.GetByIdAsync(id, cancellationToken);
		}

		public Task<IEnumerable<Student>> GetStudents(CancellationToken cancellationToken)
		{
			return _repo.GetAllAsync(cancellationToken);
		}

		public async Task<Student?> UpdateStudent(Student student, CancellationToken cancellationToken)
		{
			await _repo.UpdateAsync(student, cancellationToken);
			return student;
		}

		public Task<bool> ValidateAsync(StudentApi.Dtos.CreateUpdateStudentDto dto)
		{
			if (String.IsNullOrWhiteSpace(dto.Name)) // validate Name
				return Task.FromResult(false);

			if (dto.Age <= 0)   // Validate Age
				return Task.FromResult(false);

			if (!dto.Email.Contains("@"))   // Validate Email
				return Task.FromResult(false);

			return Task.FromResult(true);
		}
	}
}
