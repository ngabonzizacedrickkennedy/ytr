using AutoMapper;
using BCrypt.Net;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _userRepository.ExistsByUsernameAsync(username);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _userRepository.ExistsByEmailAsync(email);
        }

        public async Task<UserDTO> RegisterUserAsync(UserDTO userDTO)
        {
            // Check if username or email already exists
            if (await _userRepository.ExistsByUsernameAsync(userDTO.Username))
            {
                throw new InvalidOperationException("Username is already taken!");
            }

            if (await _userRepository.ExistsByEmailAsync(userDTO.Email))
            {
                throw new InvalidOperationException("Email is already in use!");
            }

            // Create new user
            var user = new User
            {
                UserName = userDTO.Username,
                Email = userDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                PhoneNumber = userDTO.PhoneNumber,
                Role = UserRole.ROLE_USER // Default role for new registrations
            };

            var savedUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDTO>(savedUser);
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDTO)
        {
            // Check if username or email already exists
            if (await _userRepository.ExistsByUsernameAsync(userDTO.Username))
            {
                throw new InvalidOperationException("Username is already taken!");
            }

            if (await _userRepository.ExistsByEmailAsync(userDTO.Email))
            {
                throw new InvalidOperationException("Email is already in use!");
            }

            // Create new user (similar to registerUser but allows setting custom roles)
            var user = new User
            {
                UserName = userDTO.Username,
                Email = userDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password ?? throw new ArgumentException("Password is required")),
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                PhoneNumber = userDTO.PhoneNumber,
                Role = userDTO.Role ?? UserRole.ROLE_USER // Use the role from userDTO or default to ROLE_USER
            };

            var savedUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDTO>(savedUser);
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        public async Task<UserDTO?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        public async Task<UserDTO?> UpdateUserAsync(long id, UserDTO userDTO)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            // Update username if provided and different
            if (!string.IsNullOrWhiteSpace(userDTO.Username) && user.UserName != userDTO.Username)
            {
                // Check if new username is already taken
                if (await _userRepository.ExistsByUsernameAsync(userDTO.Username))
                {
                    throw new InvalidOperationException("Username is already taken!");
                }
                user.UserName = userDTO.Username;
            }

            // Update email if provided and different
            if (!string.IsNullOrWhiteSpace(userDTO.Email) && user.Email != userDTO.Email)
            {
                // Check if new email is already taken
                if (await _userRepository.ExistsByEmailAsync(userDTO.Email))
                {
                    throw new InvalidOperationException("Email is already in use!");
                }
                user.Email = userDTO.Email;
            }

            // Update other fields
            if (!string.IsNullOrWhiteSpace(userDTO.FirstName))
                user.FirstName = userDTO.FirstName;

            if (!string.IsNullOrWhiteSpace(userDTO.LastName))
                user.LastName = userDTO.LastName;

            if (userDTO.PhoneNumber != null)
                user.PhoneNumber = userDTO.PhoneNumber;

            // Update role if provided
            if (userDTO.Role.HasValue)
                user.Role = userDTO.Role.Value;

            // Only update password if provided
            if (!string.IsNullOrWhiteSpace(userDTO.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
            }

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> UpdateUserRoleAsync(long id, UserRole role)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            user.Role = role;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return _mapper.Map<UserDTO>(user);
        }

        public async Task DeleteUserAsync(long id)
        {
            if (!await _userRepository.ExistsAsync(id))
            {
                throw new InvalidOperationException($"User not found with id: {id}");
            }

            await _userRepository.DeleteAsync(id);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _userRepository.FindByEmailAsync(email);
        }
    }
}