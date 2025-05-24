using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class AdminUsersApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IBookingService _bookingService;

        public AdminUsersApiController(IUserService userService, IBookingService bookingService)
        {
            _userService = userService;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Get all users with optional filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDTO>>>> GetAllUsers(
            [FromQuery] string? search = null,
            [FromQuery] string? role = null,
            [FromQuery] string sortBy = "username",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 0,
            [FromQuery] int size = 50)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                // Filter by search query if provided
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower().Trim();
                    users = users.Where(user =>
                        user.Username.ToLower().Contains(searchLower) ||
                        user.Email.ToLower().Contains(searchLower) ||
                        $"{user.FirstName} {user.LastName}".ToLower().Contains(searchLower))
                        .ToList();
                }

                // Filter by role if provided
                if (!string.IsNullOrEmpty(role))
                {
                    if (Enum.TryParse<UserRole>(role, out var userRole))
                    {
                        users = users.Where(user => user.Role == userRole).ToList();
                    }
                }

                // Sort users
                users = sortBy.ToLower() switch
                {
                    "username" => sortOrder.ToLower() == "desc"
                        ? users.OrderByDescending(u => u.Username).ToList()
                        : users.OrderBy(u => u.Username).ToList(),
                    "email" => sortOrder.ToLower() == "desc"
                        ? users.OrderByDescending(u => u.Email).ToList()
                        : users.OrderBy(u => u.Email).ToList(),
                    "fullname" => sortOrder.ToLower() == "desc"
                        ? users.OrderByDescending(u => $"{u.FirstName} {u.LastName}").ToList()
                        : users.OrderBy(u => $"{u.FirstName} {u.LastName}").ToList(),
                    "role" => sortOrder.ToLower() == "desc"
                        ? users.OrderByDescending(u => u.Role).ToList()
                        : users.OrderBy(u => u.Role).ToList(),
                    _ => users.OrderBy(u => u.Username).ToList()
                };

                return Ok(ApiResponse<List<UserDTO>>.SuccessResult(users));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<UserDTO>>.ErrorResult($"Failed to retrieve users: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a user by ID with their bookings
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AdminUserDetailResponse>>> GetUserById(long id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<AdminUserDetailResponse>.ErrorResult($"User not found with id: {id}"));
                }

                var response = new AdminUserDetailResponse
                {
                    User = user
                };

                // Get user's bookings
                try
                {
                    response.Bookings = await _bookingService.GetBookingsByUserAsync(id);
                }
                catch (Exception)
                {
                    // If bookings fail, still return user data
                    response.Bookings = new List<BookingDTO>();
                }

                return Ok(ApiResponse<AdminUserDetailResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AdminUserDetailResponse>.ErrorResult($"Failed to retrieve user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDTO>>> CreateUser([FromBody] UserDTO userDTO)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(userDTO.Username))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Username is required"));
                }

                if (string.IsNullOrEmpty(userDTO.Email))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Email is required"));
                }

                if (string.IsNullOrEmpty(userDTO.Password))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Password is required"));
                }

                // Check if username already exists
                if (await _userService.ExistsByUsernameAsync(userDTO.Username))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Username is already taken"));
                }

                // Check if email already exists
                if (await _userService.ExistsByEmailAsync(userDTO.Email))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult("Email is already in use"));
                }

                // Set default role if not provided
                if (!userDTO.Role.HasValue)
                {
                    userDTO.Role = UserRole.ROLE_USER;
                }

                var createdUser = await _userService.CreateUserAsync(userDTO);
                return CreatedAtAction(nameof(GetUserById),
                    new { id = createdUser.Id },
                    ApiResponse<UserDTO>.SuccessResult(createdUser, "User created successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UserDTO>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserDTO>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDTO>.ErrorResult($"Failed to create user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateUser(long id, [FromBody] UserDTO userDTO)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(id, userDTO);
                if (updatedUser == null)
                {
                    return NotFound(ApiResponse<UserDTO>.ErrorResult($"User not found with id: {id}"));
                }

                return Ok(ApiResponse<UserDTO>.SuccessResult(updatedUser, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserDTO>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDTO>.ErrorResult($"Failed to update user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update user role
        /// </summary>
        [HttpPut("{id}/role")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateUserRole(long id, [FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                if (!Enum.TryParse<UserRole>(request.Role, out var role))
                {
                    return BadRequest(ApiResponse<UserDTO>.ErrorResult($"Invalid role: {request.Role}"));
                }

                var updatedUser = await _userService.UpdateUserRoleAsync(id, role);
                if (updatedUser == null)
                {
                    return NotFound(ApiResponse<UserDTO>.ErrorResult($"User not found with id: {id}"));
                }

                return Ok(ApiResponse<UserDTO>.SuccessResult(updatedUser, "User role updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDTO>.ErrorResult($"Failed to update user role: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(long id)
        {
            try
            {
                // Check if user exists
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"User not found with id: {id}"));
                }

                // Prevent deletion of admin users (optional safety measure)
                if (user.Role == UserRole.ROLE_ADMIN)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Cannot delete admin users"));
                }

                await _userService.DeleteUserAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(null, "User deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResult($"Failed to delete user: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<UserStatsResponse>>> GetUserStats()
        {
            try
            {
                var allUsers = await _userService.GetAllUsersAsync();

                var stats = new UserStatsResponse
                {
                    TotalUsers = allUsers.Count,
                    AdminUsers = allUsers.Count(u => u.Role == UserRole.ROLE_ADMIN),
                    ManagerUsers = allUsers.Count(u => u.Role == UserRole.ROLE_MANAGER),
                    RegularUsers = allUsers.Count(u => u.Role == UserRole.ROLE_USER)
                };

                return Ok(ApiResponse<UserStatsResponse>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserStatsResponse>.ErrorResult($"Failed to retrieve user statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        [HttpGet("check-username")]
        public async Task<ActionResult<ApiResponse<UsernameCheckResponse>>> CheckUsername([FromQuery] string username)
        {
            try
            {
                var exists = await _userService.ExistsByUsernameAsync(username);
                var result = new UsernameCheckResponse
                {
                    Exists = exists,
                    Available = !exists
                };

                return Ok(ApiResponse<UsernameCheckResponse>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsernameCheckResponse>.ErrorResult($"Failed to check username: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("check-email")]
        public async Task<ActionResult<ApiResponse<EmailCheckResponse>>> CheckEmail([FromQuery] string email)
        {
            try
            {
                var exists = await _userService.ExistsByEmailAsync(email);
                var result = new EmailCheckResponse
                {
                    Exists = exists,
                    Available = !exists
                };

                return Ok(ApiResponse<EmailCheckResponse>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EmailCheckResponse>.ErrorResult($"Failed to check email: {ex.Message}"));
            }
        }
    }

    public class AdminUserDetailResponse
    {
        public UserDTO User { get; set; } = new();
        public List<BookingDTO> Bookings { get; set; } = new();
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    public class UserStatsResponse
    {
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ManagerUsers { get; set; }
        public int RegularUsers { get; set; }
    }

    public class UsernameCheckResponse
    {
        public bool Exists { get; set; }
        public bool Available { get; set; }
    }

    public class EmailCheckResponse
    {
        public bool Exists { get; set; }
        public bool Available { get; set; }
    }
}