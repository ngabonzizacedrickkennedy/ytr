using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class HomeApiController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IScreeningService _screeningService;

        public HomeApiController(IMovieService movieService, IScreeningService screeningService)
        {
            _movieService = movieService;
            _screeningService = screeningService;
        }

        [HttpGet("home")]
        public ActionResult<ApiResponse<string>> WelcomePage()
        {
            return Ok(ApiResponse<string>.SuccessResult("Welcome to Theatre Management System"));
        }

        [HttpGet("dashboard")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DashboardResponse>>> GetDashboard()
        {
            try
            {
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet();
                var response = new DashboardResponse();

                // Check user roles and redirect accordingly
                if (roles.Contains("ROLE_ADMIN") || roles.Contains("ROLE_MANAGER"))
                {
                    response.Redirect = "/admin";
                    response.IsAdmin = true;
                    response.UserRole = roles.Contains("ROLE_ADMIN") ? "ADMIN" : "MANAGER";
                    return Ok(ApiResponse<DashboardResponse>.SuccessResult(response));
                }

                // For regular users, return the home page data
                var now = DateTime.Now;
                var upcomingScreenings = await _screeningService.GetUpcomingScreeningsAsync(now);

                // Group screenings by movie
                var screeningsByMovie = upcomingScreenings
                    .GroupBy(s => s.MovieId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Get all current movies that have screenings
                var moviesWithScreenings = await _movieService.GetMoviesByIdsAsync(screeningsByMovie.Keys);

                // Get upcoming movies (with future release dates)
                var upcomingMovies = await _movieService.GetUpcomingMoviesAsync();

                response.NowPlaying = moviesWithScreenings;
                response.ScreeningsByMovie = screeningsByMovie;
                response.Upcoming = upcomingMovies;
                response.IsAdmin = false;
                response.UserRole = "USER";

                return Ok(ApiResponse<DashboardResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                // If there's an error, return basic response
                var response = new DashboardResponse
                {
                    NowPlaying = new List<MovieDTO>(),
                    ScreeningsByMovie = new Dictionary<long, List<ScreeningDTO>>(),
                    Upcoming = new List<MovieDTO>(),
                    IsAdmin = false,
                    UserRole = "USER"
                };

                return Ok(ApiResponse<DashboardResponse>.SuccessResult(response));
            }
        }

        [HttpGet("about")]
        public ActionResult<ApiResponse<AboutResponse>> GetAbout()
        {
            var aboutInfo = new AboutResponse
            {
                Name = "Theatre Management System",
                Description = "A state-of-the-art system for managing theatre operations",
                Version = "1.0.0"
            };

            return Ok(ApiResponse<AboutResponse>.SuccessResult(aboutInfo));
        }

        [HttpGet("contact")]
        public ActionResult<ApiResponse<string>> GetContact()
        {
            return Ok(ApiResponse<string>.SuccessResult("Contact form is available"));
        }

        [HttpPost("contact")]
        public ActionResult<ApiResponse<string>> ProcessContactForm([FromBody] ContactRequestDTO contactRequest)
        {
            try
            {
                // In a real application, you would process the form data here
                // For example, send an email or save to database

                return Ok(ApiResponse<string>.SuccessResult(null, "Thank you for your message! We'll get back to you shortly."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Error processing contact form: {ex.Message}"));
            }
        }
    }

    public class DashboardResponse
    {
        public List<MovieDTO> NowPlaying { get; set; } = new();
        public Dictionary<long, List<ScreeningDTO>> ScreeningsByMovie { get; set; } = new();
        public List<MovieDTO> Upcoming { get; set; } = new();
        public bool IsAdmin { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public string? Redirect { get; set; }
    }

    public class AboutResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}