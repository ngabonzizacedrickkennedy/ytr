namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOtpEmailAsync(string email, string otp);
        Task<bool> SendPasswordResetEmailAsync(string email, string token);
    }
}
