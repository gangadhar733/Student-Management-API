using StudentApi.Models;

namespace StudentApi.Repositories;

public interface IStudentRepository
{
	Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken);
	Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<Student> AddAsync(Student student, CancellationToken cancellationToken);
	Task DeleteAsync(int id, CancellationToken cancellationToken);
	Task UpdateAsync(Student student, CancellationToken cancellationToken);
}
