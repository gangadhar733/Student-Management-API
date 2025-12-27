using StudentApi.Models;

namespace StudentApi.Services
{
	public interface IUserService
	{
		public Task CreateUser(User user, CancellationToken cancellationToken);

		public Task DeleteUser(User user, CancellationToken cancellationToken);

		public Task<User?> GetUserByUsername(string username, CancellationToken cancellationToken);

		public Task<bool> IsExistingUser(string username, CancellationToken cancellationToken);
	}
}
