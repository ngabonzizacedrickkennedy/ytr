using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
    public class AdminBookingApiController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;
        private readonly IMovieService _movieService;
        private readonly ITheatreService _theatreService;

        public AdminBookingApiController(
            IBookingService bookingService,
            IUserService userService,
            IMovieService movieService,
            ITheatreService theatreService)
        {
            _bookingService = bookingService;
            _userService = userService;
            _movieService = movieService;
            _theatreService = theatreService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<BookingDTO>>>> GetAllBookings(
            [FromQuery] long? userId = null,
            [FromQuery] long? movieId = null,
            [FromQuery] long? theatreId = null,
            [FromQuery] string? bookingNumber = null,
            [FromQuery] PaymentStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                List<BookingDTO> bookings;

                // Handle different filtering options
                if (userId.HasValue)
                {
                    bookings = await _bookingService.GetBookingsByUserAsync(userId.Value);
                }
                else if (movieId.HasValue)
                {
                    bookings = await _bookingService.GetBookingsByMovieAsync(movieId.Value);
                }
                else if (theatreId.HasValue)
                {
                    bookings = await _bookingService.GetBookingsByTheatreAsync(theatreId.Value);
                }
                else if (!string.IsNullOrEmpty(bookingNumber))
                {
                    var booking = await _bookingService.GetBookingByNumberAsync(bookingNumber);
                    bookings = booking != null ? new List<BookingDTO> { booking } : new List<BookingDTO>();
                }
                else if (status.HasValue)
                {
                    bookings = await _bookingService.GetBookingsByStatusAsync(status.Value);
                }
                else if (fromDate.HasValue && toDate.HasValue)
                {
                    bookings = await _bookingService.GetBookingsByDateRangeAsync(fromDate.Value, toDate.Value);
                }
                else
                {
                    bookings = await _bookingService.GetAllBookingsAsync();
                }

                return Ok(ApiResponse<List<BookingDTO>>.SuccessResult(bookings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BookingDTO>>.ErrorResult($"Error retrieving bookings: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BookingDTO>>> GetBooking(long id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingDTO>.ErrorResult($"Booking not found with id: {id}"));
                }

                return Ok(ApiResponse<BookingDTO>.SuccessResult(booking));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BookingDTO>.ErrorResult($"Error retrieving booking: {ex.Message}"));
            }
        }

        [HttpPost("{id}/update-status")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateBookingStatus(
            long id,
            [FromBody] UpdateBookingStatusRequest request)
        {
            try
            {
                await _bookingService.UpdateBookingStatusAsync(id, request.Status);
                return Ok(ApiResponse<object>.SuccessResult(null, "Booking status updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating booking: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteBooking(long id)
        {
            try
            {
                await _bookingService.DeleteBookingAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(null, "Booking deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting booking: {ex.Message}"));
            }
        }
    }

    public class UpdateBookingStatusRequest
    {
        public PaymentStatus Status { get; set; }
    }
}