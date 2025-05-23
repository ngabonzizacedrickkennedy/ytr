using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    public class SeatMapper
    {
        private readonly IMapper _mapper;

        public SeatMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public SeatDTO ToDTO(Seat seat)
        {
            return seat == null ? null! : _mapper.Map<SeatDTO>(seat);
        }

        public Seat ToEntity(SeatDTO seatDTO)
        {
            if (seatDTO == null)
                return null!;

            var seat = _mapper.Map<Seat>(seatDTO);
            seat.Id = seatDTO.Id ?? 0;
            return seat;
        }

        public void UpdateEntityFromDTO(SeatDTO seatDTO, Seat seat)
        {
            if (seatDTO == null || seat == null)
                return;

            var originalId = seat.Id;
            var originalTheatre = seat.Theatre;

            _mapper.Map(seatDTO, seat);

            seat.Id = originalId;
            seat.Theatre = originalTheatre;
        }

        public IEnumerable<SeatDTO> ToDTO(IEnumerable<Seat> seats)
        {
            return seats?.Select(ToDTO) ?? Enumerable.Empty<SeatDTO>();
        }
    }
}
