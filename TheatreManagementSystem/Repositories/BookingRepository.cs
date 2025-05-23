using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Data;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .ToListAsync();
        }

        public override async Task<Booking?> GetByIdAsync(long id)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> FindByUserIdAsync(long userId)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> FindByScreeningIdAsync(long screeningId)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.ScreeningId == screeningId)
                .ToListAsync();
        }

        public async Task<Booking?> FindByBookingNumberAsync(string bookingNumber)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);
        }

        public async Task<IEnumerable<Booking>> FindByUserIdAndBookingTimeAfterAsync(long userId, DateTime date)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.UserId == userId && b.BookingTime > date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> FindByMovieIdAsync(long movieId)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.Screening.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> FindByTheatreIdAsync(long theatreId)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.Screening.TheatreId == theatreId)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> FindBookedSeatsByScreeningIdAsync(long screeningId)
        {
            var bookings = await _dbSet
                .Where(b => b.ScreeningId == screeningId && b.PaymentStatus != PaymentStatus.CANCELLED)
                .ToListAsync();

            var bookedSeats = new List<string>();
            foreach (var booking in bookings)
            {
                if (!string.IsNullOrEmpty(booking.BookedSeats))
                {
                    bookedSeats.AddRange(booking.BookedSeatsCollection);
                }
            }

            return bookedSeats.Distinct();
        }

        public async Task<IEnumerable<Booking>> FindByPaymentStatusAsync(PaymentStatus status)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.PaymentStatus == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> FindByBookingTimeBetweenAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(b => b.User)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Screening)
                    .ThenInclude(s => s.Theatre)
                .Where(b => b.BookingTime >= fromDate && b.BookingTime <= toDate)
                .ToListAsync();
        }
    }
}