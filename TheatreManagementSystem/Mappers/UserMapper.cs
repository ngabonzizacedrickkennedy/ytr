using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    /// <summary>
    /// Dedicated mapper class for User entities
    /// Provides additional security and control over user mapping
    /// </summary>
    public class UserMapper
    {
        private readonly IMapper _mapper;

        public UserMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Convert User entity to UserDTO (safe mapping - excludes sensitive data)
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>UserDTO</returns>
        public UserDTO ToDTO(User user)
        {
            if (user == null)
                return null!;

            var dto = _mapper.Map<UserDTO>(user);
            // Ensure password is never included in DTO
            dto.Password = null;
            return dto;
        }

        /// <summary>
        /// Convert UserDTO to User entity (for creation)
        /// </summary>
        /// <param name="userDTO">UserDTO</param>
        /// <returns>User entity</returns>
        public User ToEntity(UserDTO userDTO)
        {
            if (userDTO == null)
                return null!;

            var user = _mapper.Map<User>(userDTO);
            // Clear Identity-specific properties that should be handled by the system
            user.Id = 0; // Let the database generate the ID
            user.NormalizedUserName = null;
            user.NormalizedEmail = null;
            user.EmailConfirmed = false;
            user.SecurityStamp = null;
            user.ConcurrencyStamp = null;
            user.PhoneNumberConfirmed = false;
            user.TwoFactorEnabled = false;
            user.LockoutEnd = null;
            user.LockoutEnabled = false;
            user.AccessFailedCount = 0;

            return user;
        }

        /// <summary>
        /// Update existing User entity with data from UserDTO (excludes sensitive fields)
        /// </summary>
        /// <param name="userDTO">Source DTO</param>
        /// <param name="user">Target entity to update</param>
        public void UpdateEntityFromDTO(UserDTO userDTO, User user)
        {
            if (userDTO == null || user == null)
                return;

            // Preserve critical properties
            var originalId = user.Id;
            var originalPasswordHash = user.PasswordHash;
            var originalSecurityStamp = user.SecurityStamp;
            var originalConcurrencyStamp = user.ConcurrencyStamp;
            var originalBookings = user.Bookings;
            var originalNormalizedUserName = user.NormalizedUserName;
            var originalNormalizedEmail = user.NormalizedEmail;
            var originalEmailConfirmed = user.EmailConfirmed;
            var originalPhoneNumberConfirmed = user.PhoneNumberConfirmed;
            var originalTwoFactorEnabled = user.TwoFactorEnabled;
            var originalLockoutEnd = user.LockoutEnd;
            var originalLockoutEnabled = user.LockoutEnabled;
            var originalAccessFailedCount = user.AccessFailedCount;

            // Map the DTO to entity
            _mapper.Map(userDTO, user);

            // Restore preserved properties
            user.Id = originalId;
            user.Bookings = originalBookings;
            user.NormalizedUserName = originalNormalizedUserName;
            user.NormalizedEmail = originalNormalizedEmail;
            user.EmailConfirmed = originalEmailConfirmed;
            user.SecurityStamp = originalSecurityStamp;
            user.ConcurrencyStamp = originalConcurrencyStamp;
            user.PhoneNumberConfirmed = originalPhoneNumberConfirmed;
            user.TwoFactorEnabled = originalTwoFactorEnabled;
            user.LockoutEnd = originalLockoutEnd;
            user.LockoutEnabled = originalLockoutEnabled;
            user.AccessFailedCount = originalAccessFailedCount;

            // Only update password hash if a new password was provided
            if (string.IsNullOrEmpty(userDTO.Password))
            {
                user.PasswordHash = originalPasswordHash;
            }
            // If password is provided, it should be hashed in the service layer
        }

        /// <summary>
        /// Convert a collection of User entities to UserDTOs
        /// </summary>
        /// <param name="users">Collection of User entities</param>
        /// <returns>Collection of UserDTOs</returns>
        public IEnumerable<UserDTO> ToDTO(IEnumerable<User> users)
        {
            return users?.Select(ToDTO) ?? Enumerable.Empty<UserDTO>();
        }

        /// <summary>
        /// Create a safe UserDTO for public display (minimal information)
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>Minimal UserDTO</returns>
        public UserDTO ToPublicDTO(User user)
        {
            if (user == null)
                return null!;

            return new UserDTO
            {
                Id = user.Id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
                // Exclude email, phone, and other sensitive information
            };
        }
    }
}