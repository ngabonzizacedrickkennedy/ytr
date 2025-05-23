using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<UserDTO> RegisterUserAsync(UserDTO userDTO);
        Task<UserDTO> CreateUserAsync(UserDTO userDTO);
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(long id);
        Task<UserDTO?> GetUserByUsernameAsync(string username);
        Task<UserDTO?> UpdateUserAsync(long id, UserDTO userDTO);
        Task<UserDTO?> UpdateUserRoleAsync(long id, UserRole role);
        Task DeleteUserAsync(long id);
        Task<User?> FindByUsernameAsync(string username);
        Task<User?> FindByEmailAsync(string email);
    }
}