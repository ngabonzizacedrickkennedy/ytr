using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/theatres")]
    [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
    public class AdminTheatreApiController : ControllerBase
    {
        private readonly ITheatreService _theatreService;
        private readonly IScreeningService _screeningService;
        private readonly ISeatService _seatService;

        public AdminTheatreApiController(
            ITheatreService theatreService,
            IScreeningService screeningService,
            ISeatService seatService)
        {
            _theatreService = theatreService;
            _screeningService = screeningService;
            _seatService = seatService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TheatreDTO>>>> GetTheatres(
            [FromQuery] string? search = null,
            [FromQuery] string sortBy = "id")
        {
            try
            {
                List<TheatreDTO> theatres;

                // Handle search functionality
                if (!string.IsNullOrEmpty(search))
                {
                    theatres = await _theatreService.SearchTheatresByNameAsync(search);
                }
                else
                {
                    theatres = await _theatreService.GetAllTheatresAsync();
                }

                return Ok(ApiResponse<List<TheatreDTO>>.SuccessResult(theatres));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TheatreDTO>>.ErrorResult($"Error retrieving theatres: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TheatreDTO>>> CreateTheatre([FromBody] TheatreDTO theatreDTO)
        {
            try
            {
                var createdTheatre = await _theatreService.CreateTheatreAsync(theatreDTO);
                return CreatedAtAction(nameof(GetTheatre),
                    new { id = createdTheatre.Id },
                    ApiResponse<TheatreDTO>.SuccessResult(createdTheatre, "Theatre created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TheatreDTO>.ErrorResult($"Error creating theatre: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TheatreDTO>>> GetTheatre(long id)
        {
            try
            {
                var theatre = await _theatreService.GetTheatreByIdAsync(id);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<TheatreDTO>.ErrorResult($"Theatre not found with id: {id}"));
                }

                return Ok(ApiResponse<TheatreDTO>.SuccessResult(theatre));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TheatreDTO>.ErrorResult($"Error retrieving theatre: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<TheatreDTO>>> UpdateTheatre(long id, [FromBody] TheatreDTO theatreDTO)
        {
            try
            {
                var updatedTheatre = await _theatreService.UpdateTheatreAsync(id, theatreDTO);
                if (updatedTheatre == null)
                {
                    return NotFound(ApiResponse<TheatreDTO>.ErrorResult($"Theatre not found with id: {id}"));
                }

                return Ok(ApiResponse<TheatreDTO>.SuccessResult(updatedTheatre, "Theatre updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TheatreDTO>.ErrorResult($"Error updating theatre: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ROLE_ADMIN")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteTheatre(long id)
        {
            try
            {
                // Check if theatre exists
                var theatre = await _theatreService.GetTheatreByIdAsync(id);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Theatre not found with id: {id}"));
                }

                await _theatreService.DeleteTheatreAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(null, "Theatre deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting theatre: {ex.Message}"));
            }
        }

        [HttpGet("{id}/seats")]
        public async Task<ActionResult<ApiResponse<TheatreSeatsResponse>>> GetTheatreSeats(long id)
        {
            try
            {
                var theatre = await _theatreService.GetTheatreByIdAsync(id);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<TheatreSeatsResponse>.ErrorResult($"Theatre not found with id: {id}"));
                }

                var response = new TheatreSeatsResponse
                {
                    Theatre = theatre,
                    Seats = new Dictionary<int, object>()
                };

                // For each screen, get the seats
                if (theatre.TotalScreens.HasValue)
                {
                    for (int i = 1; i <= theatre.TotalScreens.Value; i++)
                    {
                        var seats = await _seatService.GetSeatsByTheatreAndScreenAsync(id, i);
                        response.Seats[i] = seats;
                    }
                }

                return Ok(ApiResponse<TheatreSeatsResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TheatreSeatsResponse>.ErrorResult($"Error retrieving theatre seats: {ex.Message}"));
            }
        }

        [HttpPost("{id}/seats/initialize")]
        public async Task<ActionResult<ApiResponse<object>>> InitializeSeats(
            long id,
            [FromQuery] int screenNumber,
            [FromQuery] int rows,
            [FromQuery] int seatsPerRow)
        {
            try
            {
                // Check if theatre exists
                var theatre = await _theatreService.GetTheatreByIdAsync(id);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Theatre not found with id: {id}"));
                }

                await _seatService.InitializeSeatsForTheatreAsync(id, screenNumber, rows, seatsPerRow);

                return Ok(ApiResponse<object>.SuccessResult(null,
                    $"Seats initialized for Screen {screenNumber} successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error initializing seats: {ex.Message}"));
            }
        }

        [HttpGet("{id}/screenings")]
        public async Task<ActionResult<ApiResponse<TheatreScreeningsResponse>>> GetTheatreScreenings(long id)
        {
            try
            {
                var theatre = await _theatreService.GetTheatreByIdAsync(id);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<TheatreScreeningsResponse>.ErrorResult($"Theatre not found with id: {id}"));
                }

                var screenings = await _screeningService.GetScreeningsByTheatreAsync(id);

                var response = new TheatreScreeningsResponse
                {
                    Theatre = theatre,
                    Screenings = screenings
                };

                return Ok(ApiResponse<TheatreScreeningsResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TheatreScreeningsResponse>.ErrorResult($"Error retrieving theatre screenings: {ex.Message}"));
            }
        }
    }

    public class TheatreSeatsResponse
    {
        public TheatreDTO Theatre { get; set; } = new();
        public Dictionary<int, object> Seats { get; set; } = new();
    }

    public class TheatreScreeningsResponse
    {
        public TheatreDTO Theatre { get; set; } = new();
        public List<ScreeningDTO> Screenings { get; set; } = new();
    }
}