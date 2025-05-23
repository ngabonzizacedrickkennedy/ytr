using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> FindByUsernameAsync(string username);
        Task<User?> FindByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);

        // Search users by multiple fields
        Task<IEnumerable<User>> FindByUsernameContainingIgnoreCaseOrEmailContainingIgnoreCaseOrFirstNameContainingIgnoreCaseOrLastNameContainingIgnoreCaseAsync(
            string username, string email, string firstName, string lastName);

        // Find users by role
        Task<IEnumerable<User>> FindByRoleAsync(UserRole role);

        // Check if username exists excluding a specific user ID
        Task<bool> ExistsByUsernameAndIdNotAsync(string username, long id);

        // Check if email exists excluding a specific user ID
        Task<bool> ExistsByEmailAndIdNotAsync(string email, long id);

        // Find all users with pagination and sorting
        Task<(IEnumerable<User> Users, int TotalCount)> FindAllPagedAsync(int page, int pageSize, string? sortBy = null, bool descending = false);
    }
}