using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/screenings")]
    [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
    public class AdminScreeningApiController : ControllerBase
    {
        private readonly IScreeningService _screeningService;
        private readonly IMovieService _movieService;
        private readonly ITheatreService _theatreService;
        private readonly IBookingService _bookingService;

        public AdminScreeningApiController(
            IScreeningService screeningService,
            IMovieService movieService,
            ITheatreService theatreService,
            IBookingService bookingService)
        {
            _screeningService = screeningService;
            _movieService = movieService;
            _theatreService = theatreService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<AdminScreeningListResponse>>> GetScreenings(
            [FromQuery] long? movieId = null,
            [FromQuery] long? theatreId = null,
            [FromQuery] DateTime? date = null,
            [FromQuery] string? search = null,
            [FromQuery] string sortBy = "startTime",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            try
            {
                // Validate page and size parameters
                if (page < 0) page = 0;
                if (size < 1) size = 10;
                if (size > 100) size = 100; // Limit maximum page size

                // Get screenings based on filters (using non-paginated version for now)
                List<ScreeningDTO> screenings;
                if (movieId.HasValue && theatreId.HasValue && date.HasValue)
                {
                    screenings = await _screeningService.GetScreeningsAsync(movieId, theatreId, date);
                }
                else if (movieId.HasValue && theatreId.HasValue)
                {
                    screenings = await _screeningService.GetScreeningsByMovieAndTheatreAsync(movieId.Value, theatreId.Value);
                }
                else if (movieId.HasValue)
                {
                    screenings = await _screeningService.GetScreeningsByMovieAsync(movieId.Value);
                }
                else if (theatreId.HasValue)
                {
                    screenings = await _screeningService.GetScreeningsByTheatreAsync(theatreId.Value, date);
                }
                else if (date.HasValue)
                {
                    var endDate = date.Value.AddDays(1);
                    screenings = await _screeningService.GetScreeningsByDateRangeAsync(date.Value, endDate);
                }
                else
                {
                    screenings = await _screeningService.GetAllScreeningsAsync();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(search))
                {
                    screenings = screenings.Where(s =>
                        (s.MovieTitle != null && s.MovieTitle.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (s.TheatreName != null && s.TheatreName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                // Apply sorting
                screenings = sortBy.ToLower() switch
                {
                    "starttime" => sortOrder.ToLower() == "desc"
                        ? screenings.OrderByDescending(s => s.StartTime).ToList()
                        : screenings.OrderBy(s => s.StartTime).ToList(),
                    "endtime" => sortOrder.ToLower() == "desc"
                        ? screenings.OrderByDescending(s => s.EndTime).ToList()
                        : screenings.OrderBy(s => s.EndTime).ToList(),
                    "movietitle" => sortOrder.ToLower() == "desc"
                        ? screenings.OrderByDescending(s => s.MovieTitle).ToList()
                        : screenings.OrderBy(s => s.MovieTitle).ToList(),
                    "theatrename" => sortOrder.ToLower() == "desc"
                        ? screenings.OrderByDescending(s => s.TheatreName).ToList()
                        : screenings.OrderBy(s => s.TheatreName).ToList(),
                    _ => screenings.OrderBy(s => s.StartTime).ToList()
                };

                // Apply pagination
                var totalCount = screenings.Count;
                var pagedScreenings = screenings.Skip(page * size).Take(size).ToList();

                var response = new AdminScreeningListResponse
                {
                    Screenings = pagedScreenings,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)totalCount / size),
                    TotalElements = totalCount,
                    PageSize = size,
                    HasNext = (page + 1) * size < totalCount,
                    HasPrevious = page > 0,
                    IsFirst = page == 0,
                    IsLast = (page + 1) * size >= totalCount,
                    Movies = await _movieService.GetAllMoviesAsync(),
                    Theatres = await _theatreService.GetAllTheatresAsync()
                };

                // Add filter values if provided
                if (movieId.HasValue) response.SelectedMovieId = movieId;
                if (theatreId.HasValue) response.SelectedTheatreId = theatreId;
                if (date.HasValue) response.SelectedDate = date;
                if (!string.IsNullOrEmpty(search)) response.Search = search;

                return Ok(ApiResponse<AdminScreeningListResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminScreeningListResponse>.ErrorResult($"Error retrieving screenings: {ex.Message}"));
            }
        }

        [HttpGet("formats")]
        public ActionResult<ApiResponse<List<string>>> GetFormats()
        {
            try
            {
                var formats = Enum.GetNames<ScreeningFormat>().ToList();
                return Ok(ApiResponse<List<string>>.SuccessResult(formats));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Error retrieving formats: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AdminScreeningDetailResponse>>> GetScreening(long id)
        {
            try
            {
                var screening = await _screeningService.GetScreeningByIdAsync(id);
                if (screening == null)
                {
                    return NotFound(ApiResponse<AdminScreeningDetailResponse>.ErrorResult($"Screening not found with id: {id}"));
                }

                var bookedSeats = await _bookingService.GetBookedSeatsByScreeningIdAsync(id);

                var response = new AdminScreeningDetailResponse
                {
                    Screening = screening,
                    BookedSeats = bookedSeats
                };

                return Ok(ApiResponse<AdminScreeningDetailResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminScreeningDetailResponse>.ErrorResult($"Error retrieving screening: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ScreeningDTO>>> CreateScreening([FromBody] ScreeningDTO screeningDTO)
        {
            try
            {
                // Handle date/time conversion if needed
                if (!string.IsNullOrEmpty(screeningDTO.StartDateString) && !string.IsNullOrEmpty(screeningDTO.StartTimeString))
                {
                    if (DateTime.TryParse($"{screeningDTO.StartDateString}T{screeningDTO.StartTimeString}:00", out var combinedDateTime))
                    {
                        screeningDTO.StartTime = combinedDateTime;
                    }
                    else
                    {
                        return BadRequest(ApiResponse<ScreeningDTO>.ErrorResult("Invalid date or time format"));
                    }
                }
                else if (screeningDTO.StartTime == default)
                {
                    return BadRequest(ApiResponse<ScreeningDTO>.ErrorResult("Start date and time are required"));
                }

                // Create the screening
                var createdScreening = await _screeningService.CreateScreeningAsync(screeningDTO);
                return CreatedAtAction(nameof(GetScreening),
                    new { id = createdScreening.Id },
                    ApiResponse<ScreeningDTO>.SuccessResult(createdScreening, "Screening created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ScreeningDTO>.ErrorResult($"Error creating screening: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ScreeningDTO>>> UpdateScreening(long id, [FromBody] ScreeningDTO screeningDTO)
        {
            try
            {
                // Handle date/time conversion if needed
                if (!string.IsNullOrEmpty(screeningDTO.StartDateString) && !string.IsNullOrEmpty(screeningDTO.StartTimeString))
                {
                    if (DateTime.TryParse($"{screeningDTO.StartDateString}T{screeningDTO.StartTimeString}:00", out var combinedDateTime))
                    {
                        screeningDTO.StartTime = combinedDateTime;
                    }
                }

                var updatedScreening = await _screeningService.UpdateScreeningAsync(id, screeningDTO);
                if (updatedScreening == null)
                {
                    return NotFound(ApiResponse<ScreeningDTO>.ErrorResult($"Screening not found with id: {id}"));
                }

                return Ok(ApiResponse<ScreeningDTO>.SuccessResult(updatedScreening, "Screening updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ScreeningDTO>.ErrorResult($"Error updating screening: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteScreening(long id)
        {
            try
            {
                // Check if screening exists
                var screening = await _screeningService.GetScreeningByIdAsync(id);
                if (screening == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Screening not found with id: {id}"));
                }

                await _screeningService.DeleteScreeningAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(null, "Screening deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting screening: {ex.Message}"));
            }
        }

        [HttpGet("{id}/bookings")]
        public async Task<ActionResult<ApiResponse<List<BookingDTO>>>> GetScreeningBookings(long id)
        {
            try
            {
                // Check if screening exists
                var screening = await _screeningService.GetScreeningByIdAsync(id);
                if (screening == null)
                {
                    return NotFound(ApiResponse<List<BookingDTO>>.ErrorResult($"Screening not found with id: {id}"));
                }

                var bookings = await _bookingService.GetBookingsByScreeningIdAsync(id);
                return Ok(ApiResponse<List<BookingDTO>>.SuccessResult(bookings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BookingDTO>>.ErrorResult($"Error retrieving screening bookings: {ex.Message}"));
            }
        }
    }

    public class AdminScreeningListResponse
    {
        public List<ScreeningDTO> Screenings { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalElements { get; set; }
        public int PageSize { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public List<MovieDTO> Movies { get; set; } = new();
        public List<TheatreDTO> Theatres { get; set; } = new();
        public long? SelectedMovieId { get; set; }
        public long? SelectedTheatreId { get; set; }
        public DateTime? SelectedDate { get; set; }
        public string? Search { get; set; }
    }

    public class AdminScreeningDetailResponse
    {
        public ScreeningDTO Screening { get; set; } = new();
        public HashSet<string> BookedSeats { get; set; } = new();
    }
}