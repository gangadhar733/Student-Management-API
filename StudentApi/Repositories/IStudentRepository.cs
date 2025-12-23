using StudentApi.Models;

namespace StudentApi.Repositories;

public interface IStudentRepository
{
	Task<IEnumerable<Student>> GetAllAsync();
	Task<Student?> GetByIdAsync(int id);
	Task<Student> AddAsync(Student student);
	Task DeleteAsync(int id);
	Task UpdateAsync(Student student);
}
