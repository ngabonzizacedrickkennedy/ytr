using System.ComponentModel.DataAnnotations;

namespace TheatreManagementSystem.DTOs.Auth
{
    public class PasswordResetRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
}
