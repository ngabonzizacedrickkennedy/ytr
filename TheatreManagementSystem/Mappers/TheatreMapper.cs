using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    namespace TheatreManagementSystem.Mappers
    {
        /// <summary>
        /// Dedicated mapper class for Theatre entities
        /// </summary>
        public class TheatreMapper
        {
            private readonly IMapper _mapper;

            public TheatreMapper(IMapper mapper)
            {
                _mapper = mapper;
            }

            public TheatreDTO ToDTO(Theatre theatre)
            {
                return theatre == null ? null! : _mapper.Map<TheatreDTO>(theatre);
            }

            public Theatre ToEntity(TheatreDTO theatreDTO)
            {
                if (theatreDTO == null)
                    return null!;

                var theatre = _mapper.Map<Theatre>(theatreDTO);
                theatre.Id = theatreDTO.Id ?? 0; // Let database generate ID if not provided
                return theatre;
            }

            public void UpdateEntityFromDTO(TheatreDTO theatreDTO, Theatre theatre)
            {
                if (theatreDTO == null || theatre == null)
                    return;

                var originalId = theatre.Id;
                var originalScreenings = theatre.Screenings;
                var originalSeats = theatre.Seats;

                _mapper.Map(theatreDTO, theatre);

                theatre.Id = originalId;
                theatre.Screenings = originalScreenings;
                theatre.Seats = originalSeats;
            }

            public IEnumerable<TheatreDTO> ToDTO(IEnumerable<Theatre> theatres)
            {
                return theatres?.Select(ToDTO) ?? Enumerable.Empty<TheatreDTO>();
            }
        }

        /// <summary>
        /// Dedicated mapper class for Screening entities
        /// </summary>
    }
}
