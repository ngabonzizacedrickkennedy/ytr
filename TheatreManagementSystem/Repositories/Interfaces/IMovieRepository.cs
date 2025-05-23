using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Repositories.Interfaces
{
    public interface IMovieRepository : IGenericRepository<Movie>
    {
        // Search methods
        Task<IEnumerable<Movie>> FindByTitleContainingIgnoreCaseAsync(string title);
        Task<IEnumerable<Movie>> FindByGenreAsync(MovieGenre genre);
        Task<IEnumerable<Movie>> FindByReleaseDateAfterAsync(DateTime date);
        Task<IEnumerable<Movie>> FindUpcomingMoviesAsync(DateTime currentDate);

        // Pagination methods
        Task<(IEnumerable<Movie> Movies, int TotalCount)> FindByTitleContainingIgnoreCasePagedAsync(string title, int page, int pageSize);
        Task<(IEnumerable<Movie> Movies, int TotalCount)> FindByGenrePagedAsync(MovieGenre genre, int page, int pageSize);
        Task<(IEnumerable<Movie> Movies, int TotalCount)> FindByReleaseDateAfterPagedAsync(DateTime date, int page, int pageSize);
        Task<(IEnumerable<Movie> Movies, int TotalCount)> FindUpcomingMoviesPagedAsync(DateTime currentDate, int page, int pageSize);

        // Advanced search with filters
        Task<(IEnumerable<Movie> Movies, int TotalCount)> FindMoviesWithFiltersPagedAsync(
            string? title = null,
            MovieGenre? genre = null,
            int page = 0,
            int pageSize = 10);

        // Global search methods
        Task<IEnumerable<Movie>> FindByTitleContainingIgnoreCaseOrDirectorContainingIgnoreCaseOrCastContainingIgnoreCaseAsync(
            string title, string director, string cast, int limit = 10);

        // Advanced search with multiple criteria
        Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int limit = 10);

        // Get movies by IDs
        Task<IEnumerable<Movie>> FindByIdsAsync(IEnumerable<long> ids);
    }
}