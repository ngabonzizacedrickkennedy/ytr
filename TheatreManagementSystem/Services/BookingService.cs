using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly ISeatRepository _seatRepository;
        private readonly IMapper _mapper;

        public BookingService(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IScreeningRepository screeningRepository,
            ISeatRepository seatRepository,
            IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _screeningRepository = screeningRepository;
            _seatRepository = seatRepository;
            _mapper = mapper;
        }

        public async Task<BookingDTO> CreateBookingAsync(long screeningId, string username, List<string> selectedSeats, string paymentMethod)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var screening = await _screeningRepository.GetByIdAsync(screeningId);
            if (screening == null)
                throw new InvalidOperationException("Screening not found");

            // Check if seats are available
            var bookedSeats = await _bookingRepository.FindBookedSeatsByScreeningIdAsync(screeningId);
            var bookedSeatsSet = new HashSet<string>(bookedSeats);

            foreach (var seat in selectedSeats)
            {
                if (bookedSeatsSet.Contains(seat))
                {
                    throw new InvalidOperationException($"Seat {seat} is already booked");
                }
            }

            // Calculate total price
            var totalPrice = await CalculateTotalPriceAsync(screeningId, selectedSeats);

            // Create booking
            var booking = new Booking
            {
                UserId = user.Id,
                ScreeningId = screeningId,
                BookingNumber = GenerateBookingNumber(),
                BookingTime = DateTime.Now,
                TotalAmount = totalPrice,
                PaymentStatus = PaymentStatus.COMPLETED, // Assuming payment is done immediately
                BookedSeatsCollection = selectedSeats
            };

            var savedBooking = await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            return await ConvertToDTOAsync(savedBooking);
        }

        public async Task<List<BookingDTO>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        public async Task<BookingDTO?> GetBookingByIdAsync(long id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking != null ? await ConvertToDTOAsync(booking) : null;
        }

        public async Task<BookingDTO?> GetBookingByNumberAsync(string bookingNumber)
        {
            var booking = await _bookingRepository.FindByBookingNumberAsync(bookingNumber);
            return booking != null ? await ConvertToDTOAsync(booking) : null;
        }

        public async Task<List<BookingDTO>> GetBookingsByUserAsync(long userId)
        {
            var bookings = await _bookingRepository.FindByUserIdAsync(userId);
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        public async Task<List<BookingDTO>> GetBookingsByUsernameAsync(string username)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null)
                throw new InvalidOperationException("User not found");

            return await GetBookingsByUserAsync(user.Id);
        }

        public async Task<HashSet<string>> GetBookedSeatsByScreeningIdAsync(long screeningId)
        {
            var bookedSeatsList = await _bookingRepository.FindBookedSeatsByScreeningIdAsync(screeningId);
            return new HashSet<string>(bookedSeatsList);
        }

        public async Task CancelBookingAsync(long id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new InvalidOperationException("Booking not found");

            // Check if the screening is in the future
            if (booking.Screening.StartTime < DateTime.Now)
            {
                throw new InvalidOperationException("Cannot cancel past bookings");
            }

            booking.PaymentStatus = PaymentStatus.CANCELLED;
            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task<double> CalculateTotalPriceAsync(long screeningId, List<string> selectedSeats)
        {
            var screening = await _screeningRepository.GetByIdAsync(screeningId);
            if (screening == null)
                throw new InvalidOperationException("Screening not found");

            var basePrice = screening.BasePrice;
            var totalPrice = 0.0;

            // Get all seats from this theatre and screen
            var seats = await _seatRepository.FindByTheatreIdAndScreenNumberAsync(
                screening.TheatreId, screening.ScreenNumber);

            var seatMap = seats.ToDictionary(
                seat => $"{seat.RowName}{seat.SeatNumber}",
                seat => seat
            );

            // Calculate price based on seat type
            foreach (var seatKey in selectedSeats)
            {
                if (seatMap.TryGetValue(seatKey, out var seat))
                {
                    totalPrice += basePrice * seat.PriceMultiplier;
                }
                else
                {
                    totalPrice += basePrice; // Default to base price if seat not found
                }
            }

            return totalPrice;
        }

        public async Task<List<BookingDTO>> GetBookingsByScreeningIdAsync(long screeningId)
        {
            var bookings = await _bookingRepository.FindByScreeningIdAsync(screeningId);
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        // Admin functionality
        public async Task<List<BookingDTO>> GetBookingsByMovieAsync(long movieId)
        {
            var bookings = await _bookingRepository.FindByMovieIdAsync(movieId);
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        public async Task<List<BookingDTO>> GetBookingsByTheatreAsync(long theatreId)
        {
            var bookings = await _bookingRepository.FindByTheatreIdAsync(theatreId);
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        public async Task<List<BookingDTO>> GetBookingsByStatusAsync(PaymentStatus status)
        {
            var bookings = await _bookingRepository.FindByPaymentStatusAsync(status);
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        public async Task<List<BookingDTO>> GetBookingsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var bookings = await _bookingRepository.FindByBookingTimeBetweenAsync(fromDate, toDate);
            var bookingDTOs = new List<BookingDTO>();

            foreach (var booking in bookings)
            {
                bookingDTOs.Add(await ConvertToDTOAsync(booking));
            }

            return bookingDTOs;
        }

        public async Task UpdateBookingStatusAsync(long id, PaymentStatus status)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new InvalidOperationException($"Booking not found with id: {id}");

            booking.PaymentStatus = status;
            await _bookingRepository.UpdateAsync(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task DeleteBookingAsync(long id)
        {
            await _bookingRepository.DeleteAsync(id);
            await _bookingRepository.SaveChangesAsync();
        }

        // Helper methods
        private string GenerateBookingNumber()
        {
            return $"BK{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{new Random().Next(1000)}";
        }

        private async Task<BookingDTO> ConvertToDTOAsync(Booking booking)
        {
            var dto = new BookingDTO
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber,
                UserId = booking.UserId,
                Username = booking.User?.UserName,
                UserEmail = booking.User?.Email,
                ScreeningId = booking.ScreeningId,
                MovieTitle = booking.Screening?.Movie?.Title,
                MovieId = booking.Screening?.MovieId,
                TheatreId = booking.Screening?.TheatreId,
                MovieUrl = booking.Screening?.Movie?.TrailerUrl,
                TheatreName = booking.Screening?.Theatre?.Name,
                ScreeningTime = booking.Screening?.StartTime ?? DateTime.MinValue,
                BookingTime = booking.BookingTime,
                TotalAmount = booking.TotalAmount,
                PaymentStatus = booking.PaymentStatus,
                BookedSeats = booking.BookedSeatsCollection
            };

            return dto;
        }
    }
}