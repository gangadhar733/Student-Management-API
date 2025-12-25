using StudentApi.Models;
namespace StudentApi.Services
{
	public interface IStudentService
	{
		Task<IEnumerable<Student>> GetStudents(CancellationToken cancellation);
		Task<Student?> GetStudent(int id, CancellationToken cancellationToken);
		Task<Student> CreateStudent(Student student, CancellationToken cancellationToken);
		Task<bool> DeleteStudent(int id, CancellationToken cancellationToken);
		Task<Student?> UpdateStudent(Student student, CancellationToken cancellationToken);
		Task<bool> ValidateAsync(StudentApi.Dtos.CreateUpdateStudentDto Dto);
	}
}
