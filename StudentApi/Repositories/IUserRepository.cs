using StudentApi.Models;

namespace StudentApi.Repositories;

public interface IUserRepository
{
	public Task<bool> AddAsync(User user, CancellationToken cancellationToken);

	public Task<bool> DeleteAsync(User user, CancellationToken cancellationToken);

	public Task<User?> GetUserAsync(string username, CancellationToken cancellationToken);

	public Task<bool> IsExistingUserAsync(string username, CancellationToken cancellationToken);
}
