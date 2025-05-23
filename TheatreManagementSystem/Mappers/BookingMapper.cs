using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    public class BookingMapper
    {
        private readonly IMapper _mapper;

        public BookingMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public BookingDTO ToDTO(Booking booking)
        {
            return booking == null ? null! : _mapper.Map<BookingDTO>(booking);
        }

        public Booking ToEntity(BookingDTO bookingDTO)
        {
            if (bookingDTO == null)
                return null!;

            var booking = _mapper.Map<Booking>(bookingDTO);
            booking.Id = bookingDTO.Id ?? 0;
            return booking;
        }

        public void UpdateEntityFromDTO(BookingDTO bookingDTO, Booking booking)
        {
            if (bookingDTO == null || booking == null)
                return;

            var originalId = booking.Id;
            var originalUser = booking.User;
            var originalScreening = booking.Screening;

            _mapper.Map(bookingDTO, booking);

            booking.Id = originalId;
            booking.User = originalUser;
            booking.Screening = originalScreening;
        }

        public IEnumerable<BookingDTO> ToDTO(IEnumerable<Booking> bookings)
        {
            return bookings?.Select(ToDTO) ?? Enumerable.Empty<BookingDTO>();
        }
    }

    /// <summary>
    /// Dedicated mapper class for Seat entities
    /// </summary>
}
