using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface ITheatreService
    {
        Task<TheatreDTO> CreateTheatreAsync(TheatreDTO theatreDTO);
        Task<List<TheatreDTO>> GetAllTheatresAsync();
        Task<TheatreDTO?> GetTheatreByIdAsync(long id);
        Task<List<TheatreDTO>> SearchTheatresByNameAsync(string name);
        Task<List<TheatreDTO>> SearchTheatresByAddressAsync(string address);
        Task<List<TheatreDTO>> GetTheatresByMovieAsync(long movieId);
        Task<TheatreDTO?> UpdateTheatreAsync(long id, TheatreDTO theatreDTO);
        Task DeleteTheatreAsync(long id);
        Task<Theatre?> GetTheatreEntityByIdAsync(long id);
    }
}