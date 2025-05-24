using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class AdminApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMovieService _movieService;
        private readonly ITheatreService _theatreService;
        private readonly IBookingService _bookingService;

        public AdminApiController(
            IUserService userService,
            IMovieService movieService,
            ITheatreService theatreService,
            IBookingService bookingService)
        {
            _userService = userService;
            _movieService = movieService;
            _theatreService = theatreService;
            _bookingService = bookingService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<AdminDashboardResponse>>> GetDashboard()
        {
            try
            {
                var dashboardData = new AdminDashboardResponse();

                // Total counts for dashboard cards
                var allUsers = await _userService.GetAllUsersAsync();
                var allMovies = await _movieService.GetAllMoviesAsync();
                var allTheatres = await _theatreService.GetAllTheatresAsync();
                var allBookings = await _bookingService.GetAllBookingsAsync();

                dashboardData.TotalUsers = allUsers.Count;
                dashboardData.TotalMovies = allMovies.Count;
                dashboardData.TotalTheatres = allTheatres.Count;
                dashboardData.TotalBookings = allBookings.Count;

                // Get recent bookings (last 5)
                var recentBookings = allBookings
                    .OrderByDescending(b => b.BookingTime)
                    .Take(5)
                    .ToList();
                dashboardData.RecentBookings = recentBookings;

                // Get new users count (simplified)
                dashboardData.NewUsersCount = allUsers.Count;

                // Get popular movies (first 5 for now)
                var popularMovies = allMovies.Take(5).ToList();
                dashboardData.PopularMovies = popularMovies;

                // Get upcoming screenings (next 24 hours)
                var now = DateTime.Now;
                var tomorrow = now.AddDays(1);
                var upcomingBookings = await _bookingService.GetBookingsByDateRangeAsync(now, tomorrow);
                dashboardData.UpcomingScreenings = upcomingBookings;

                // Get booking statistics by status
                var completedBookings = allBookings.Count(b => b.PaymentStatus == PaymentStatus.COMPLETED);
                var pendingBookings = allBookings.Count(b => b.PaymentStatus == PaymentStatus.PENDING);
                var cancelledBookings = allBookings.Count(b => b.PaymentStatus == PaymentStatus.CANCELLED);

                var bookingStats = new Dictionary<string, long>
                {
                    ["completed"] = completedBookings,
                    ["pending"] = pendingBookings,
                    ["cancelled"] = cancelledBookings
                };
                dashboardData.BookingStats = bookingStats;

                return Ok(ApiResponse<AdminDashboardResponse>.SuccessResult(dashboardData));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminDashboardResponse>.ErrorResult($"Error retrieving dashboard data: {ex.Message}"));
            }
        }
    }

    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int TotalMovies { get; set; }
        public int TotalTheatres { get; set; }
        public int TotalBookings { get; set; }
        public List<BookingDTO> RecentBookings { get; set; } = new();
        public int NewUsersCount { get; set; }
        public List<MovieDTO> PopularMovies { get; set; } = new();
        public List<BookingDTO> UpcomingScreenings { get; set; } = new();
        public Dictionary<string, long> BookingStats { get; set; } = new();
    }
}