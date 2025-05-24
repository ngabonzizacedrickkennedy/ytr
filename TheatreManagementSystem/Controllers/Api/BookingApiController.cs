
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingApiController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingApiController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<BookingDTO>>>> GetUserBookings()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<List<BookingDTO>>.ErrorResult("User not authenticated"));
                }

                var bookings = await _bookingService.GetBookingsByUsernameAsync(username);
                return Ok(ApiResponse<List<BookingDTO>>.SuccessResult(bookings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BookingDTO>>.ErrorResult($"Error retrieving bookings: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BookingDTO>>> GetBookingById(long id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<BookingDTO>.ErrorResult("User not authenticated"));
                }

                // Check if the booking exists
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound(ApiResponse<BookingDTO>.ErrorResult($"Booking not found with id: {id}"));
                }

                // Check if the user is allowed to access this booking
                var isAdmin = User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_MANAGER");
                if (!isAdmin && booking.Username != username)
                {
                    return Forbid();
                }

                return Ok(ApiResponse<BookingDTO>.SuccessResult(booking));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BookingDTO>.ErrorResult($"Error retrieving booking: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BookingDTO>>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<BookingDTO>.ErrorResult("User not authenticated"));
                }

                // Validate request
                if (request.ScreeningId <= 0)
                {
                    return BadRequest(ApiResponse<BookingDTO>.ErrorResult("Screening ID is required"));
                }

                if (request.SelectedSeats == null || !request.SelectedSeats.Any())
                {
                    return BadRequest(ApiResponse<BookingDTO>.ErrorResult("Selected seats are required"));
                }

                if (string.IsNullOrEmpty(request.PaymentMethod))
                {
                    return BadRequest(ApiResponse<BookingDTO>.ErrorResult("Payment method is required"));
                }

                var createdBooking = await _bookingService.CreateBookingAsync(
                    request.ScreeningId,
                    username,
                    request.SelectedSeats,
                    request.PaymentMethod);

                return CreatedAtAction(nameof(GetBookingById),
                    new { id = createdBooking.Id },
                    ApiResponse<BookingDTO>.SuccessResult(createdBooking, "Booking created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BookingDTO>.ErrorResult($"Error creating booking: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> CancelBooking(long id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResult("User not authenticated"));
                }

                // Check if the booking exists
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Booking not found with id: {id}"));
                }

                // Check if the user is allowed to cancel this booking
                var isAdmin = User.IsInRole("ROLE_ADMIN") || User.IsInRole("ROLE_MANAGER");
                if (!isAdmin && booking.Username != username)
                {
                    return Forbid();
                }

                await _bookingService.CancelBookingAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(null, "Booking cancelled successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error cancelling booking: {ex.Message}"));
            }
        }

        [HttpGet("calculate-price")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<PriceCalculationResponse>>> CalculatePrice(
            [FromQuery] long screeningId,
            [FromQuery] string seats)
        {
            try
            {
                if (string.IsNullOrEmpty(seats))
                {
                    return BadRequest(ApiResponse<PriceCalculationResponse>.ErrorResult("Seats parameter is required"));
                }

                var selectedSeats = seats.Split(',').ToList();
                var totalPrice = await _bookingService.CalculateTotalPriceAsync(screeningId, selectedSeats);

                var response = new PriceCalculationResponse
                {
                    ScreeningId = screeningId,
                    SelectedSeats = selectedSeats,
                    TotalPrice = totalPrice
                };

                return Ok(ApiResponse<PriceCalculationResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PriceCalculationResponse>.ErrorResult($"Error calculating price: {ex.Message}"));
            }
        }

        // Admin only endpoints
        [HttpGet("admin/all")]
        [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
        public async Task<ActionResult<ApiResponse<List<BookingDTO>>>> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return Ok(ApiResponse<List<BookingDTO>>.SuccessResult(bookings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<BookingDTO>>.ErrorResult($"Error retrieving all bookings: {ex.Message}"));
            }
        }
    }

    public class CreateBookingRequest
    {
        public long ScreeningId { get; set; }
        public List<string> SelectedSeats { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class PriceCalculationResponse
    {
        public long ScreeningId { get; set; }
        public List<string> SelectedSeats { get; set; } = new();
        public double TotalPrice { get; set; }
    }
}