using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.DTOs.Auth;
using TheatreManagementSystem.Security;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserDetailsService _userDetailsService;
        private readonly JwtTokenProvider _tokenProvider;

        public AuthApiController(
            IUserService userService,
            UserDetailsService userDetailsService,
            JwtTokenProvider tokenProvider)
        {
            _userService = userService;
            _userDetailsService = userDetailsService;
            _tokenProvider = tokenProvider;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Validate credentials
                var isValid = await _userDetailsService.ValidateCredentialsAsync(loginRequest.Username, loginRequest.Password);
                if (!isValid)
                {
                    return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Invalid username or password"));
                }

                // Load user and create principal
                var user = await _userDetailsService.LoadUserByUsernameAsync(loginRequest.Username);
                if (user == null)
                {
                    return BadRequest(ApiResponse<LoginResponse>.ErrorResult("User not found"));
                }

                var principal = await _userDetailsService.CreateAsync(user);

                // Generate JWT token
                var jwt = _tokenProvider.GenerateToken(principal);

                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var loginResponse = new LoginResponse(jwt, user.UserName ?? "", string.Join(",", roles));

                return Ok(ApiResponse<LoginResponse>.SuccessResult(loginResponse, "User logged in successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult($"Login failed: {ex.Message}"));
            }
        }

        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Signup([FromBody] SignupRequest signupRequest)
        {
            try
            {
                // Check if username already exists
                if (await _userService.ExistsByUsernameAsync(signupRequest.Username))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Username is already taken!"));
                }

                // Check if email already exists
                if (await _userService.ExistsByEmailAsync(signupRequest.Email))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Email is already in use!"));
                }

                // Create new user
                var userDTO = new UserDTO
                {
                    Username = signupRequest.Username,
                    Email = signupRequest.Email,
                    Password = signupRequest.Password,
                    FirstName = signupRequest.FirstName,
                    LastName = signupRequest.LastName,
                    PhoneNumber = signupRequest.PhoneNumber
                };

                var createdUser = await _userService.RegisterUserAsync(userDTO);

                return CreatedAtAction(nameof(GetProfile),
                    null,
                    ApiResponse<UserDTO>.SuccessResult(createdUser, "User registered successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDTO>.ErrorResult($"Registration failed: {ex.Message}"));
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetProfile()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<UserDTO>.ErrorResult("User not authenticated"));
                }

                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDTO>.ErrorResult("User not found"));
                }

                return Ok(ApiResponse<UserDTO>.SuccessResult(user));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDTO>.ErrorResult($"Error retrieving profile: {ex.Message}"));
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateProfile([FromBody] UserDTO userDTO)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<UserDTO>.ErrorResult("User not authenticated"));
                }

                var currentUser = await _userService.GetUserByUsernameAsync(username);
                if (currentUser == null)
                {
                    return NotFound(ApiResponse<UserDTO>.ErrorResult("User not found"));
                }

                var updatedUser = await _userService.UpdateUserAsync(currentUser.Id!.Value, userDTO);
                if (updatedUser == null)
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Failed to update user"));
                }

                return Ok(ApiResponse<UserDTO>.SuccessResult(updatedUser, "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDTO>.ErrorResult($"Error updating profile: {ex.Message}"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                // In JWT, logout is typically handled client-side by removing the token
                // But we can perform any server-side cleanup here if needed
                return Ok(ApiResponse<object>.SuccessResult(new object(), "Logged out successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error during logout: {ex.Message}"));
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<LoginResponse>.ErrorResult("User not authenticated"));
                }

                var user = await _userDetailsService.LoadUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(ApiResponse<LoginResponse>.ErrorResult("User not found"));
                }

                var principal = await _userDetailsService.CreateAsync(user);
                var jwt = _tokenProvider.GenerateToken(principal);

                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var loginResponse = new LoginResponse(jwt, user.UserName ?? "", string.Join(",", roles));

                return Ok(ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Token refreshed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult($"Token refresh failed: {ex.Message}"));
            }
        }
    }
}