using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Repositories.Interfaces
{
    public interface ITheatreRepository : IGenericRepository<Theatre>
    {
        Task<IEnumerable<Theatre>> FindByNameContainingIgnoreCaseAsync(string name);
        Task<IEnumerable<Theatre>> FindByAddressContainingIgnoreCaseAsync(string address);
        Task<IEnumerable<Theatre>> FindTheatresByMovieIdAsync(long movieId);

        // New search methods for global search
        Task<IEnumerable<Theatre>> FindByNameContainingIgnoreCaseOrAddressContainingIgnoreCaseAsync(string name, string address);

        Task<IEnumerable<Theatre>> SearchTheatresAsync(string query);
    }
}