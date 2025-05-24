using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.DTOs.Auth;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/auth/password")]
    public class PasswordResetApiController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;

        public PasswordResetApiController(IPasswordResetService passwordResetService)
        {
            _passwordResetService = passwordResetService;
        }

        /// <summary>
        /// Initiate password reset by sending reset link to user's email
        /// </summary>
        [HttpPost("forgot")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] PasswordResetRequest request)
        {
            try
            {
                Console.WriteLine($"Password reset request for email: {request.Email}");

                var emailSent = await _passwordResetService.InitiatePasswordResetAsync(request.Email);

                Console.WriteLine($"Password reset email sent: {emailSent}");

                // Always return success to prevent email enumeration attacks
                return Ok(ApiResponse<object>.SuccessResult(null,
                    "If an account with that email exists, a password reset link has been sent."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password reset error: {ex.Message}");
                // Still return success to prevent information disclosure
                return Ok(ApiResponse<object>.SuccessResult(null,
                    "If an account with that email exists, a password reset link has been sent."));
            }
        }

        /// <summary>
        /// Validate password reset token
        /// </summary>
        [HttpPost("validate-token")]
        public async Task<ActionResult<ApiResponse<PasswordResetValidationResponse>>> ValidateToken([FromBody] PasswordResetTokenRequest request)
        {
            try
            {
                Console.WriteLine($"Validating reset token: {request.Token}");

                var email = await _passwordResetService.ValidateResetTokenAsync(request.Token);

                Console.WriteLine($"Token validation result - email: {email}");

                if (email == null)
                {
                    Console.WriteLine("Token validation failed - token is invalid or expired");
                    return BadRequest(ApiResponse<PasswordResetValidationResponse>.ErrorResult(
                        "Invalid or expired token. Please request a new password reset link."));
                }

                var response = new PasswordResetValidationResponse
                {
                    Email = email,
                    Token = request.Token,
                    Valid = true
                };

                Console.WriteLine($"Token validation successful for email: {email}");

                return Ok(ApiResponse<PasswordResetValidationResponse>.SuccessResult(response, "Valid reset token"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                return BadRequest(ApiResponse<PasswordResetValidationResponse>.ErrorResult(
                    "Invalid or expired token. Please request a new password reset link."));
            }
        }

        /// <summary>
        /// Reset password using token
        /// </summary>
        [HttpPost("reset")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] PasswordResetTokenRequest request)
        {
            try
            {
                Console.WriteLine($"Resetting password with token: {request.Token}");

                if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
                {
                    Console.WriteLine("Password reset failed - password too short");
                    return BadRequest(ApiResponse<object>.ErrorResult("Password must be at least 6 characters long"));
                }

                var success = await _passwordResetService.ResetPasswordAsync(request.Token, request.NewPassword);

                Console.WriteLine($"Password reset result: {success}");

                if (!success)
                {
                    Console.WriteLine("Password reset failed - invalid or expired token");
                    return BadRequest(ApiResponse<object>.ErrorResult("Failed to reset password. Invalid or expired token."));
                }

                Console.WriteLine("Password reset successful");
                return Ok(ApiResponse<object>.SuccessResult(null, "Password reset successful. You can now login with your new password."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password reset error: {ex.Message}");
                return BadRequest(ApiResponse<object>.ErrorResult("Failed to reset password. Please try again."));
            }
        }
    }

    public class PasswordResetValidationResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool Valid { get; set; }
    }
}