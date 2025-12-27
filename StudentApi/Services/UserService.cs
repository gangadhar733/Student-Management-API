using StudentApi.Models;
using StudentApi.Repositories;

namespace StudentApi.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;

		public UserService(IUserRepository userRepository)
		{
			_userRepository = userRepository; 
		}
		public async Task CreateUser(User user, CancellationToken cancellationToken)
		{
			await _userRepository.AddAsync(user, cancellationToken);
		}

		public async Task DeleteUser(User user, CancellationToken cancellationToken)
		{
			await _userRepository.DeleteAsync(user, cancellationToken);
		}

		public async Task<User?> GetUserByUsername(string username, CancellationToken cancellationToken)
		{
			return await _userRepository.GetUserAsync(username, cancellationToken);
		}

		public async Task<bool> IsExistingUser(string username, CancellationToken cancellationToken)
		{
			return await _userRepository.IsExistingUserAsync(username, cancellationToken);
		}
	}
}
