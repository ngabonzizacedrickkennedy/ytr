using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Security
{
    public class UserDetailsService : IUserClaimsPrincipalFactory<User>
    {
        private readonly IUserRepository _userRepository;

        public UserDetailsService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.UserName ?? string.Empty),
        new(ClaimTypes.Email, user.Email ?? string.Empty),
        new(ClaimTypes.GivenName, user.FirstName),
        new(ClaimTypes.Surname, user.LastName),
        new(ClaimTypes.Role, user.Role.ToString())
    };

            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            await Task.CompletedTask; // Fix async warning
            return new ClaimsPrincipal(identity);
        }
        public async Task<User?> LoadUserByUsernameAsync(string username)
        {
            return await _userRepository.FindByUsernameAsync(username);
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<ClaimsPrincipal?> CreatePrincipalAsync(string username)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null)
                return null;

            return await CreateAsync(user);
        }
    }
}