using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    public class ScreeningMapper
    {
        private readonly IMapper _mapper;

        public ScreeningMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ScreeningDTO ToDTO(Screening screening)
        {
            return screening == null ? null! : _mapper.Map<ScreeningDTO>(screening);
        }

        public Screening ToEntity(ScreeningDTO screeningDTO)
        {
            if (screeningDTO == null)
                return null!;

            var screening = _mapper.Map<Screening>(screeningDTO);
            screening.Id = screeningDTO.Id ?? 0;
            return screening;
        }

        public void UpdateEntityFromDTO(ScreeningDTO screeningDTO, Screening screening)
        {
            if (screeningDTO == null || screening == null)
                return;

            var originalId = screening.Id;
            var originalMovie = screening.Movie;
            var originalTheatre = screening.Theatre;
            var originalBookings = screening.Bookings;

            _mapper.Map(screeningDTO, screening);

            screening.Id = originalId;
            screening.Movie = originalMovie;
            screening.Theatre = originalTheatre;
            screening.Bookings = originalBookings;
        }

        public IEnumerable<ScreeningDTO> ToDTO(IEnumerable<Screening> screenings)
        {
            return screenings?.Select(ToDTO) ?? Enumerable.Empty<ScreeningDTO>();
        }
    }

    /// <summary>
    /// Dedicated mapper class for Booking entities
    /// </summary>

    /// <summary>
    /// Dedicated mapper class for Seat entities
    /// </summary>
}
