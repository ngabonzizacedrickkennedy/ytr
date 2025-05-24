using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Security;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Configuration
{
    /// <summary>
    /// Custom authentication service for handling username/password authentication
    /// Similar to Spring Boot's AuthenticationManager
    /// </summary>
    public class CustomAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly UserDetailsService _userDetailsService;

        public CustomAuthenticationService(IUserService userService, Security.UserDetailsService userDetailsService)
        {
            _userService = userService;
            _userDetailsService = userDetailsService;
        }

        public async Task<ClaimsPrincipal?> AuthenticateAsync(string username, string password)
        {
            var isValid = await _userDetailsService.ValidateCredentialsAsync(username, password);
            if (!isValid)
                return null;

            var user = await _userDetailsService.LoadUserByUsernameAsync(username);
            if (user == null)
                return null;

            return await _userDetailsService.CreateAsync(user);
        }
    }

    /// <summary>
    /// Extension methods for authentication configuration
    /// </summary>
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddScoped<CustomAuthenticationService>();
            return services;
        }
    }
}