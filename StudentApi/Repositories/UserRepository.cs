using Microsoft.EntityFrameworkCore;
using StudentApi.Data;
using StudentApi.Models;

namespace StudentApi.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<bool> AddAsync(User user, CancellationToken cancellationToken)
		{
			_context.Users.Add(user);
			await _context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> DeleteAsync(User user, CancellationToken cancellationToken)
		{
			var existingUser = await GetUserAsync(user.Username, cancellationToken);//await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username, cancellationToken);

			if (user is null)
				return false;

			_context.Users.Remove(user);
			await _context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<User?> GetUserAsync(string username, CancellationToken cancellationToken)
		{
			return await _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
		}

		public async Task<bool> IsExistingUserAsync(string username, CancellationToken cancellationToken)
		{
			return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
		}
	}
}
