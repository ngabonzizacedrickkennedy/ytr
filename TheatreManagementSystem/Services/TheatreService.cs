using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Services
{
    public class TheatreService : ITheatreService
    {
        private readonly ITheatreRepository _theatreRepository;
        private readonly IMapper _mapper;

        public TheatreService(ITheatreRepository theatreRepository, IMapper mapper)
        {
            _theatreRepository = theatreRepository;
            _mapper = mapper;
        }

        public async Task<TheatreDTO> CreateTheatreAsync(TheatreDTO theatreDTO)
        {
            var theatre = _mapper.Map<Theatre>(theatreDTO);
            var savedTheatre = await _theatreRepository.AddAsync(theatre);
            await _theatreRepository.SaveChangesAsync();
            return _mapper.Map<TheatreDTO>(savedTheatre);
        }

        public async Task<List<TheatreDTO>> GetAllTheatresAsync()
        {
            var theatres = await _theatreRepository.GetAllAsync();
            return _mapper.Map<List<TheatreDTO>>(theatres);
        }

        public async Task<TheatreDTO?> GetTheatreByIdAsync(long id)
        {
            var theatre = await _theatreRepository.GetByIdAsync(id);
            return theatre != null ? _mapper.Map<TheatreDTO>(theatre) : null;
        }

        public async Task<List<TheatreDTO>> SearchTheatresByNameAsync(string name)
        {
            var theatres = await _theatreRepository.FindByNameContainingIgnoreCaseAsync(name);
            return _mapper.Map<List<TheatreDTO>>(theatres);
        }

        public async Task<List<TheatreDTO>> SearchTheatresByAddressAsync(string address)
        {
            var theatres = await _theatreRepository.FindByAddressContainingIgnoreCaseAsync(address);
            return _mapper.Map<List<TheatreDTO>>(theatres);
        }

        public async Task<List<TheatreDTO>> GetTheatresByMovieAsync(long movieId)
        {
            var theatres = await _theatreRepository.FindTheatresByMovieIdAsync(movieId);
            return _mapper.Map<List<TheatreDTO>>(theatres);
        }

        public async Task<TheatreDTO?> UpdateTheatreAsync(long id, TheatreDTO theatreDTO)
        {
            var theatre = await _theatreRepository.GetByIdAsync(id);
            if (theatre == null)
                return null;

            theatre.Name = theatreDTO.Name;
            theatre.Address = theatreDTO.Address;
            theatre.PhoneNumber = theatreDTO.PhoneNumber;
            theatre.Email = theatreDTO.Email;
            theatre.Description = theatreDTO.Description;
            theatre.TotalScreens = theatreDTO.TotalScreens;
            theatre.ImageUrl = theatreDTO.ImageUrl;

            await _theatreRepository.UpdateAsync(theatre);
            await _theatreRepository.SaveChangesAsync();

            return _mapper.Map<TheatreDTO>(theatre);
        }

        public async Task DeleteTheatreAsync(long id)
        {
            await _theatreRepository.DeleteAsync(id);
            await _theatreRepository.SaveChangesAsync();
        }

        public async Task<Theatre?> GetTheatreEntityByIdAsync(long id)
        {
            return await _theatreRepository.GetByIdAsync(id);
        }
    }
}