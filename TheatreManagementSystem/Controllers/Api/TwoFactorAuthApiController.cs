using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheatreManagementSystem.Configuration;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.DTOs.Auth;
using TheatreManagementSystem.Security;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/auth/2fa")]
    public class TwoFactorAuthApiController : ControllerBase
    {
        private readonly CustomAuthenticationService _authenticationService;
        private readonly JwtTokenProvider _tokenProvider;
        private readonly IUserService _userService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public TwoFactorAuthApiController(
            CustomAuthenticationService authenticationService,
            JwtTokenProvider tokenProvider,
            IUserService userService,
            ITwoFactorAuthService twoFactorAuthService)
        {
            _authenticationService = authenticationService;
            _tokenProvider = tokenProvider;
            _userService = userService;
            _twoFactorAuthService = twoFactorAuthService;
        }

        /// <summary>
        /// Initiate login with 2FA
        /// First authenticate username and password, then send OTP
        /// </summary>
        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponse<TwoFactorInitiateResponse>>> InitiateLogin([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Step 1: Authenticate username and password
                var principal = await _authenticationService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);
                if (principal == null)
                {
                    return BadRequest(ApiResponse<TwoFactorInitiateResponse>.ErrorResult("Invalid username or password"));
                }

                // Step 2: Get user details
                var user = await _userService.FindByUsernameAsync(loginRequest.Username);
                if (user == null)
                {
                    return BadRequest(ApiResponse<TwoFactorInitiateResponse>.ErrorResult("User not found"));
                }

                // Step 3: Generate and send OTP
                var otpSent = await _twoFactorAuthService.GenerateAndSendOtpAsync(user);
                if (!otpSent)
                {
                    return BadRequest(ApiResponse<TwoFactorInitiateResponse>.ErrorResult("Failed to send OTP. Please try again."));
                }

                // Step 4: Return partial success, indicating that 2FA is required
                var response = new TwoFactorInitiateResponse
                {
                    Requires2FA = true,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty
                };

                return Ok(ApiResponse<TwoFactorInitiateResponse>.SuccessResult(response, "OTP sent successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TwoFactorInitiateResponse>.ErrorResult($"Authentication failed: {ex.Message}"));
            }
        }

        /// <summary>
        /// Complete login by verifying OTP
        /// </summary>
        [HttpPost("verify")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            try
            {
                // Step 1: Find user by username
                var user = await _userService.FindByUsernameAsync(request.Username);
                if (user == null)
                {
                    return BadRequest(ApiResponse<LoginResponse>.ErrorResult("User not found"));
                }

                // Step 2: Verify OTP
                var isValid = await _twoFactorAuthService.VerifyOtpAsync(user.Email!, request.Otp);
                if (!isValid)
                {
                    return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Invalid or expired OTP. Please try again."));
                }

                // Step 3: Generate authentication token
                var principal = await _authenticationService.AuthenticateAsync(request.Username, request.Password);
                if (principal == null)
                {
                    return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Authentication failed"));
                }

                var jwt = _tokenProvider.GenerateToken(principal);

                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var loginResponse = new LoginResponse(jwt, request.Username, string.Join(",", roles));

                return Ok(ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Login successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult($"OTP verification failed: {ex.Message}"));
            }
        }
    }

    public class TwoFactorInitiateResponse
    {
        public bool Requires2FA { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}