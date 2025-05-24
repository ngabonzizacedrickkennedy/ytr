using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/seats")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class AdminSeatApiController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly ITheatreService _theatreService;

        public AdminSeatApiController(ISeatService seatService, ITheatreService theatreService)
        {
            _seatService = seatService;
            _theatreService = theatreService;
        }

        [HttpGet("theatre/{theatreId}/screens")]
        public async Task<ActionResult<ApiResponse<TheatreScreensResponse>>> GetTheatreScreens(long theatreId)
        {
            try
            {
                var theatre = await _theatreService.GetTheatreEntityByIdAsync(theatreId);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<TheatreScreensResponse>.ErrorResult($"Theatre not found with id: {theatreId}"));
                }

                var response = new TheatreScreensResponse
                {
                    Theatre = theatre,
                    TotalScreens = theatre.TotalScreens ?? 0
                };

                return Ok(ApiResponse<TheatreScreensResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TheatreScreensResponse>.ErrorResult($"Error retrieving theatre screens: {ex.Message}"));
            }
        }

        [HttpGet("theatre/{theatreId}/screen/{screenNumber}")]
        public async Task<ActionResult<List<SeatDTO>>> GetSeatsByScreen(long theatreId, int screenNumber)
        {
            try
            {
                var theatre = await _theatreService.GetTheatreByIdAsync(theatreId);
                if (theatre == null)
                {
                    return NotFound();
                }

                var seats = await _seatService.GetSeatsByTheatreAndScreenAsync(theatreId, screenNumber);

                // Convert to DTOs to avoid circular references
                var seatDTOs = seats.Select(seat => new SeatDTO
                {
                    Id = seat.Id,
                    RowName = seat.RowName,
                    SeatNumber = seat.SeatNumber,
                    ScreenNumber = seat.ScreenNumber,
                    SeatType = seat.SeatType.ToString(),
                    PriceMultiplier = seat.PriceMultiplier
                }).ToList();

                return Ok(seatDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving seats: {ex.Message}");
            }
        }

        [HttpPost("theatre/{theatreId}/screen/{screenNumber}/initialize")]
        public async Task<ActionResult<ApiResponse<string>>> InitializeSeats(
            long theatreId,
            int screenNumber,
            [FromQuery] int rows,
            [FromQuery] int seatsPerRow)
        {
            try
            {
                // Check if theatre exists
                var theatre = await _theatreService.GetTheatreEntityByIdAsync(theatreId);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult($"Theatre not found with id: {theatreId}"));
                }

                await _seatService.InitializeSeatsForTheatreAsync(theatreId, screenNumber, rows, seatsPerRow);

                return Ok(ApiResponse<string>.SuccessResult(
                    null,
                    $"Successfully initialized {rows * seatsPerRow} seats for screen {screenNumber}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Error initializing seats: {ex.Message}"));
            }
        }

        [HttpPut("{seatId}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateSeat(
            long seatId,
            [FromQuery] SeatType seatType,
            [FromQuery] double priceMultiplier)
        {
            try
            {
                var seat = await _seatService.GetSeatByIdAsync(seatId);
                if (seat == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Seat not found with id: {seatId}"));
                }

                await _seatService.UpdateSeatTypeAsync(seatId, seatType, priceMultiplier);

                return Ok(ApiResponse<object>.SuccessResult(null, "Seat updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating seat: {ex.Message}"));
            }
        }

        [HttpPut("updateRow")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateSeatRow(
            [FromQuery] long theatreId,
            [FromQuery] int screenNumber,
            [FromQuery] string rowName,
            [FromQuery] SeatType seatType,
            [FromQuery] double priceMultiplier)
        {
            try
            {
                // Check if theatre exists
                var theatre = await _theatreService.GetTheatreEntityByIdAsync(theatreId);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Theatre not found with id: {theatreId}"));
                }

                await _seatService.UpdateSeatRowTypeAsync(theatreId, screenNumber, rowName, seatType, priceMultiplier);

                return Ok(ApiResponse<object>.SuccessResult(null, $"Successfully updated seats in row {rowName}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating row: {ex.Message}"));
            }
        }

        [HttpPut("bulkUpdate")]
        public async Task<ActionResult<ApiResponse<string>>> BulkUpdateSeats(
            [FromQuery] long theatreId,
            [FromQuery] int screenNumber,
            [FromQuery] string selection,
            [FromQuery] SeatType seatType,
            [FromQuery] double priceMultiplier)
        {
            try
            {
                // Check if theatre exists
                var theatre = await _theatreService.GetTheatreEntityByIdAsync(theatreId);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult($"Theatre not found with id: {theatreId}"));
                }

                var seatIds = selection.Split(',').ToList();
                var updatedCount = await _seatService.BulkUpdateSeatsAsync(
                    seatIds, theatreId, screenNumber, seatType, priceMultiplier);

                return Ok(ApiResponse<string>.SuccessResult(
                    null, $"Successfully updated {updatedCount} seats"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Error updating seats: {ex.Message}"));
            }
        }

        [HttpDelete("theatre/{theatreId}/screen/{screenNumber}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteScreenSeats(long theatreId, int screenNumber)
        {
            try
            {
                // Check if theatre exists
                var theatre = await _theatreService.GetTheatreEntityByIdAsync(theatreId);
                if (theatre == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResult($"Theatre not found with id: {theatreId}"));
                }

                var deletedCount = await _seatService.DeleteScreenSeatsAsync(theatreId, screenNumber);

                return Ok(ApiResponse<string>.SuccessResult(
                    null, $"Successfully deleted {deletedCount} seats from screen {screenNumber}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Error deleting seats: {ex.Message}"));
            }
        }
    }

    public class TheatreScreensResponse
    {
        public Theatre Theatre { get; set; } = null!;
        public int TotalScreens { get; set; }
    }
}