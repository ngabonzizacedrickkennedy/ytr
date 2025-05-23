using System.ComponentModel.DataAnnotations;

namespace TheatreManagementSystem.DTOs.Auth
{
    public class PasswordResetTokenRequest
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        // Optional for validation, required for reset
        [StringLength(40, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }
    }
}
