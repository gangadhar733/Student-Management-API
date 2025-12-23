using StudentApi.Models;
namespace StudentApi.Services
{
	public interface IStudentService
	{
		Task<IEnumerable<Student>> GetStudents();
		Task<Student?> GetStudent(int id);
		Task<Student> CreateStudent(Student student);
		Task<bool> DeleteStudent(int id);
		Task<Student?> UpdateStudent(Student student);
		Task<bool> ValidateAsync(StudentApi.Dtos.CreateUpdateStudentDto Dto);
	}
}
