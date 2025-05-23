using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Data;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> FindByUsernameContainingIgnoreCaseOrEmailContainingIgnoreCaseOrFirstNameContainingIgnoreCaseOrLastNameContainingIgnoreCaseAsync(
            string username, string email, string firstName, string lastName)
        {
            return await _dbSet
                .Where(u =>
                    u.UserName.ToLower().Contains(username.ToLower()) ||
                    u.Email.ToLower().Contains(email.ToLower()) ||
                    u.FirstName.ToLower().Contains(firstName.ToLower()) ||
                    u.LastName.ToLower().Contains(lastName.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> FindByRoleAsync(UserRole role)
        {
            return await _dbSet.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<bool> ExistsByUsernameAndIdNotAsync(string username, long id)
        {
            return await _dbSet.AnyAsync(u => u.UserName == username && u.Id != id);
        }

        public async Task<bool> ExistsByEmailAndIdNotAsync(string email, long id)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && u.Id != id);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> FindAllPagedAsync(int page, int pageSize, string? sortBy = null, bool descending = false)
        {
            var query = _dbSet.AsQueryable();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "username" => descending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                    "email" => descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "firstname" => descending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                    "lastname" => descending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                    "role" => descending ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
                    _ => descending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id)
                };
            }
            else
            {
                query = query.OrderBy(u => u.Id);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}