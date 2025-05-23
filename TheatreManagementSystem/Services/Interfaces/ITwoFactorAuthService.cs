using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface ITwoFactorAuthService
    {
        Task<bool> GenerateAndSendOtpAsync(User user);
        Task<bool> VerifyOtpAsync(string email, string otp);
    }
}
