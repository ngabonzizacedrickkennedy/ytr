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
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private const int OTP_LENGTH = 6;
        private const int OTP_VALIDITY_MINUTES = 10;

        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;

        public TwoFactorAuthService(IMemoryCache cache, IEmailService emailService)
        {
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<bool> GenerateAndSendOtpAsync(User user)
        {
            var otp = GenerateOtp();
            var expiryTime = DateTime.Now.AddMinutes(OTP_VALIDITY_MINUTES);

            // Store OTP with expiration time
            _cache.Set($"otp_{user.Email}", new { Otp = otp, ExpiryTime = expiryTime }, TimeSpan.FromMinutes(OTP_VALIDITY_MINUTES));

            // Send OTP via email
            return await _emailService.SendOtpEmailAsync(user.Email, otp);
        }

        public async Task<bool> VerifyOtpAsync(string email, string otpToVerify)
        {
            if (_cache.TryGetValue($"otp_{email}", out var cachedData))
            {
                dynamic otpData = cachedData!;

                // Check if OTP is expired
                if (DateTime.Now > otpData.ExpiryTime)
                {
                    _cache.Remove($"otp_{email}"); // Clean up expired OTP
                    return false;
                }

                // Verify OTP
                if (otpData.Otp == otpToVerify)
                {
                    _cache.Remove($"otp_{email}"); // OTP can only be used once
                    return true;
                }
            }

            return false;
        }

        private string GenerateOtp()
        {
            var otp = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < OTP_LENGTH; i++)
            {
                otp.Append(random.Next(0, 10)); // Generate random digit (0-9)
            }

            return otp.ToString();
        }
    }
}
