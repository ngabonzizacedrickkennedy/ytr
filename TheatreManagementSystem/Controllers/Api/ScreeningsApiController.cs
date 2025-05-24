using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/screenings")]
    public class ScreeningsApiController : ControllerBase
    {
        private readonly IScreeningService _screeningService;

        public ScreeningsApiController(IScreeningService screeningService)
        {
            _screeningService = screeningService;
        }

        /// <summary>
        /// Get all available screenings with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ScreeningDTO>>>> GetScreenings(
            [FromQuery] long? movieId = null,
            [FromQuery] long? theatreId = null,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var screenings = await _screeningService.GetScreeningsAsync(movieId, theatreId, date);
                return Ok(ApiResponse<List<ScreeningDTO>>.SuccessResult(screenings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ScreeningDTO>>.ErrorResult($"Error retrieving screenings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get a screening by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ScreeningDTO>>> GetScreeningById(long id)
        {
            try
            {
                var screening = await _screeningService.GetScreeningByIdAsync(id);
                if (screening == null)
                {
                    return NotFound(ApiResponse<ScreeningDTO>.ErrorResult($"Screening with id {id} not found"));
                }
                return Ok(ApiResponse<ScreeningDTO>.SuccessResult(screening));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ScreeningDTO>.ErrorResult($"Error retrieving screening: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get screenings for a specific movie
        /// </summary>
        [HttpGet("movie/{movieId}")]
        public async Task<ActionResult<ApiResponse<List<ScreeningDTO>>>> GetScreeningsByMovie(
            long movieId,
            [FromQuery] int days = 7)
        {
            try
            {
                var screenings = await _screeningService.GetScreeningsByMovieAsync(movieId, days);
                return Ok(ApiResponse<List<ScreeningDTO>>.SuccessResult(screenings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ScreeningDTO>>.ErrorResult($"Error retrieving screenings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get screenings for a specific theatre
        /// </summary>
        [HttpGet("theatre/{theatreId}")]
        public async Task<ActionResult<ApiResponse<List<ScreeningDTO>>>> GetScreeningsByTheatre(
            long theatreId,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                var screenings = await _screeningService.GetScreeningsByTheatreAsync(theatreId, date);
                return Ok(ApiResponse<List<ScreeningDTO>>.SuccessResult(screenings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ScreeningDTO>>.ErrorResult($"Error retrieving screenings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get upcoming screenings grouped by date
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, List<ScreeningDTO>>>>> GetUpcomingScreenings(
            [FromQuery] int days = 7)
        {
            try
            {
                var screenings = await _screeningService.GetUpcomingScreeningsAsync(days);
                return Ok(ApiResponse<Dictionary<string, List<ScreeningDTO>>>.SuccessResult(screenings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, List<ScreeningDTO>>>.ErrorResult($"Error retrieving upcoming screenings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get screenings within a date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<ApiResponse<List<ScreeningDTO>>>> GetScreeningsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var screenings = await _screeningService.GetScreeningsByDateRangeAsync(startDate, endDate);
                return Ok(ApiResponse<List<ScreeningDTO>>.SuccessResult(screenings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<ScreeningDTO>>.ErrorResult($"Error retrieving screenings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get available seats for a screening
        /// </summary>
        [HttpGet("{id}/seats")]
        public async Task<ActionResult<ApiResponse<HashSet<string>>>> GetAvailableSeats(long id)
        {
            try
            {
                var availableSeats = await _screeningService.GetAvailableSeatsAsync(id);
                return Ok(ApiResponse<HashSet<string>>.SuccessResult(availableSeats));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<HashSet<string>>.ErrorResult($"Error retrieving available seats: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get booked seats for a screening
        /// </summary>
        [HttpGet("{id}/booked-seats")]
        public async Task<ActionResult<ApiResponse<HashSet<string>>>> GetBookedSeats(long id)
        {
            try
            {
                var bookedSeats = await _screeningService.GetBookedSeatsAsync(id);
                return Ok(ApiResponse<HashSet<string>>.SuccessResult(bookedSeats));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<HashSet<string>>.ErrorResult($"Error retrieving booked seats: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get seating layout for a screening
        /// </summary>
        [HttpGet("{id}/layout")]
        public async Task<ActionResult<ApiResponse<object>>> GetSeatingLayout(long id)
        {
            try
            {
                var layout = await _screeningService.GetSeatingLayoutAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(layout));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving seating layout: {ex.Message}"));
            }
        }
    }
}