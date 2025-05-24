using Microsoft.Extensions.Caching.Memory;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;
using AutoMapper;
namespace TheatreManagementSystem.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private const int TOKEN_VALIDITY_HOURS = 24;

        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public PasswordResetService(IMemoryCache cache, IUserRepository userRepository, IEmailService emailService)
        {
            _cache = cache;
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<bool> InitiatePasswordResetAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
                return false; // User not found

            var token = GenerateResetToken();
            var expiryTime = DateTime.Now.AddHours(TOKEN_VALIDITY_HOURS);

            // Store token with expiration time
            _cache.Set($"reset_token_{token}", new { Email = user.Email, ExpiryTime = expiryTime }, TimeSpan.FromHours(TOKEN_VALIDITY_HOURS));

            // Send reset email
            return await _emailService.SendPasswordResetEmailAsync(user.Email, token);
        }

        public async Task<string?> ValidateResetTokenAsync(string token)
        {
            if (_cache.TryGetValue($"reset_token_{token}", out var cachedData))
            {
                dynamic tokenData = cachedData!;

                // Check if token is expired
                if (DateTime.Now > tokenData.ExpiryTime)
                {
                    _cache.Remove($"reset_token_{token}"); // Clean up expired token
                    return null;
                }

                await Task.CompletedTask; // Fix async warning
                return tokenData.Email;
            }

            await Task.CompletedTask; // Fix async warning
            return null; // Token not found
        }
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var email = await ValidateResetTokenAsync(token);
            if (email == null)
                return false; // Invalid or expired token

            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
                return false; // User not found

            var encodedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordHash = encodedPassword;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Remove used token
            _cache.Remove($"reset_token_{token}");

            return true;
        }

        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
