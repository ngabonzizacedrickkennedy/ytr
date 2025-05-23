namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task<bool> InitiatePasswordResetAsync(string email);
        Task<string?> ValidateResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
