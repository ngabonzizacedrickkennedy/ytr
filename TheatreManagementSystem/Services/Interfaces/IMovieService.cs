using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IMovieService
    {
        // Existing methods
        Task<List<MovieDTO>> GetAllMoviesAsync();
        Task<MovieDTO?> GetMovieByIdAsync(long id);
        Task<MovieDTO> CreateMovieAsync(MovieDTO movieDTO);
        Task<MovieDTO?> UpdateMovieAsync(long id, MovieDTO movieDTO);
        Task DeleteMovieAsync(long id);
        Task<List<MovieDTO>> SearchMoviesByTitleAsync(string title);
        Task<List<MovieDTO>> GetMoviesByGenreAsync(MovieGenre genre);
        Task<List<MovieDTO>> GetUpcomingMoviesAsync();
        Task<List<MovieDTO>> GetMoviesByIdsAsync(IEnumerable<long> ids);

        // New pagination methods
        Task<(List<MovieDTO> Movies, int TotalCount)> GetAllMoviesPagedAsync(int page, int pageSize);
        Task<(List<MovieDTO> Movies, int TotalCount)> SearchMoviesByTitlePagedAsync(string title, int page, int pageSize);
        Task<(List<MovieDTO> Movies, int TotalCount)> GetMoviesByGenrePagedAsync(MovieGenre genre, int page, int pageSize);
        Task<(List<MovieDTO> Movies, int TotalCount)> GetUpcomingMoviesPagedAsync(int page, int pageSize);
        Task<(List<MovieDTO> Movies, int TotalCount)> GetMoviesWithFiltersPagedAsync(string? title = null, MovieGenre? genre = null, int page = 0, int pageSize = 10);
    }
}