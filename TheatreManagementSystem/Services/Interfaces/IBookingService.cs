using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDTO> CreateBookingAsync(long screeningId, string username, List<string> selectedSeats, string paymentMethod);
        Task<List<BookingDTO>> GetAllBookingsAsync();
        Task<BookingDTO?> GetBookingByIdAsync(long id);
        Task<BookingDTO?> GetBookingByNumberAsync(string bookingNumber);
        Task<List<BookingDTO>> GetBookingsByUserAsync(long userId);
        Task<List<BookingDTO>> GetBookingsByUsernameAsync(string username);
        Task<HashSet<string>> GetBookedSeatsByScreeningIdAsync(long screeningId);
        Task CancelBookingAsync(long id);
        Task<double> CalculateTotalPriceAsync(long screeningId, List<string> selectedSeats);
        Task<List<BookingDTO>> GetBookingsByScreeningIdAsync(long screeningId);

        // Admin functionality
        Task<List<BookingDTO>> GetBookingsByMovieAsync(long movieId);
        Task<List<BookingDTO>> GetBookingsByTheatreAsync(long theatreId);
        Task<List<BookingDTO>> GetBookingsByStatusAsync(PaymentStatus status);
        Task<List<BookingDTO>> GetBookingsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task UpdateBookingStatusAsync(long id, PaymentStatus status);
        Task DeleteBookingAsync(long id);
    }
}