using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> FindByUserIdAsync(long userId);
        Task<IEnumerable<Booking>> FindByScreeningIdAsync(long screeningId);
        Task<Booking?> FindByBookingNumberAsync(string bookingNumber);
        Task<IEnumerable<Booking>> FindByUserIdAndBookingTimeAfterAsync(long userId, DateTime date);
        Task<IEnumerable<Booking>> FindByMovieIdAsync(long movieId);
        Task<IEnumerable<Booking>> FindByTheatreIdAsync(long theatreId);
        Task<IEnumerable<string>> FindBookedSeatsByScreeningIdAsync(long screeningId);
        Task<IEnumerable<Booking>> FindByPaymentStatusAsync(PaymentStatus status);
        Task<IEnumerable<Booking>> FindByBookingTimeBetweenAsync(DateTime fromDate, DateTime toDate);
    }
}