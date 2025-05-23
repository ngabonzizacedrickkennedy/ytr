using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Never map password back to DTO
                .ReverseMap()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Handle password hashing in service
                .ForMember(dest => dest.Bookings, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
                .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore());

            // Movie mappings
            CreateMap<Movie, MovieDTO>()
                .ReverseMap()
                .ForMember(dest => dest.Screenings, opt => opt.Ignore()); // Don't map navigation properties

            // Theatre mappings
            CreateMap<Theatre, TheatreDTO>()
                .ReverseMap()
                .ForMember(dest => dest.Screenings, opt => opt.Ignore())
                .ForMember(dest => dest.Seats, opt => opt.Ignore());

            // Screening mappings
            CreateMap<Screening, ScreeningDTO>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie != null ? src.Movie.Title : null))
                .ForMember(dest => dest.TheatreName, opt => opt.MapFrom(src => src.Theatre != null ? src.Theatre.Name : null))
                .ForMember(dest => dest.StartDateString, opt => opt.MapFrom(src => src.StartTime.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.StartTimeString, opt => opt.MapFrom(src => src.StartTime.ToString("HH:mm")))
                .ReverseMap()
                .ForMember(dest => dest.Movie, opt => opt.Ignore())
                .ForMember(dest => dest.Theatre, opt => opt.Ignore())
                .ForMember(dest => dest.Bookings, opt => opt.Ignore());

            // Booking mappings
            CreateMap<Booking, BookingDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Screening != null && src.Screening.Movie != null ? src.Screening.Movie.Title : null))
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Screening != null ? src.Screening.MovieId : (long?)null))
                .ForMember(dest => dest.TheatreId, opt => opt.MapFrom(src => src.Screening != null ? src.Screening.TheatreId : (long?)null))
                .ForMember(dest => dest.MovieUrl, opt => opt.MapFrom(src => src.Screening != null && src.Screening.Movie != null ? src.Screening.Movie.TrailerUrl : null))
                .ForMember(dest => dest.TheatreName, opt => opt.MapFrom(src => src.Screening != null && src.Screening.Theatre != null ? src.Screening.Theatre.Name : null))
                .ForMember(dest => dest.ScreeningTime, opt => opt.MapFrom(src => src.Screening != null ? src.Screening.StartTime : DateTime.MinValue))
                .ForMember(dest => dest.BookedSeats, opt => opt.MapFrom(src => src.BookedSeatsCollection))
                .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore()) // PaymentMethod is not stored in entity
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Screening, opt => opt.Ignore())
                .ForMember(dest => dest.BookedSeatsCollection, opt => opt.MapFrom(src => src.BookedSeats));

            // Seat mappings
            CreateMap<Seat, SeatDTO>()
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => src.SeatType.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Theatre, opt => opt.Ignore())
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => Enum.Parse<SeatType>(src.SeatType ?? "STANDARD")));
        }
    }
}