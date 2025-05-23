using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Data;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Repositories
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        public MovieRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Movie>> FindByTitleContainingIgnoreCaseAsync(string title)
        {
            return await _dbSet
                .Where(m => m.Title.ToLower().Contains(title.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> FindByGenreAsync(MovieGenre genre)
        {
            return await _dbSet.Where(m => m.Genre == genre).ToListAsync();
        }

        public async Task<IEnumerable<Movie>> FindByReleaseDateAfterAsync(DateTime date)
        {
            return await _dbSet.Where(m => m.ReleaseDate > date).ToListAsync();
        }

        public async Task<IEnumerable<Movie>> FindUpcomingMoviesAsync(DateTime currentDate)
        {
            return await _dbSet
                .Where(m => m.ReleaseDate > currentDate)
                .OrderBy(m => m.ReleaseDate)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Movie> Movies, int TotalCount)> FindByTitleContainingIgnoreCasePagedAsync(string title, int page, int pageSize)
        {
            var query = _dbSet.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movies, totalCount);
        }

        public async Task<(IEnumerable<Movie> Movies, int TotalCount)> FindByGenrePagedAsync(MovieGenre genre, int page, int pageSize)
        {
            var query = _dbSet.Where(m => m.Genre == genre);
            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movies, totalCount);
        }

        public async Task<(IEnumerable<Movie> Movies, int TotalCount)> FindByReleaseDateAfterPagedAsync(DateTime date, int page, int pageSize)
        {
            var query = _dbSet.Where(m => m.ReleaseDate > date);
            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movies, totalCount);
        }

        public async Task<(IEnumerable<Movie> Movies, int TotalCount)> FindUpcomingMoviesPagedAsync(DateTime currentDate, int page, int pageSize)
        {
            var query = _dbSet
                .Where(m => m.ReleaseDate > currentDate)
                .OrderBy(m => m.ReleaseDate);

            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movies, totalCount);
        }

        public async Task<(IEnumerable<Movie> Movies, int TotalCount)> FindMoviesWithFiltersPagedAsync(
            string? title = null, MovieGenre? genre = null, int page = 0, int pageSize = 10)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            }

            if (genre.HasValue)
            {
                query = query.Where(m => m.Genre == genre.Value);
            }

            var totalCount = await query.CountAsync();
            var movies = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (movies, totalCount);
        }

        public async Task<IEnumerable<Movie>> FindByTitleContainingIgnoreCaseOrDirectorContainingIgnoreCaseOrCastContainingIgnoreCaseAsync(
            string title, string director, string cast, int limit = 10)
        {
            return await _dbSet
                .Where(m =>
                    m.Title.ToLower().Contains(title.ToLower()) ||
                    (m.Director != null && m.Director.ToLower().Contains(director.ToLower())) ||
                    (m.Cast != null && m.Cast.ToLower().Contains(cast.ToLower())))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int limit = 10)
        {
            var searchTerm = query.ToLower();
            return await _dbSet
                .Where(m =>
                    m.Title.ToLower().Contains(searchTerm) ||
                    (m.Director != null && m.Director.ToLower().Contains(searchTerm)) ||
                    (m.Cast != null && m.Cast.ToLower().Contains(searchTerm)) ||
                    (m.Description != null && m.Description.ToLower().Contains(searchTerm)))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Movie>> FindByIdsAsync(IEnumerable<long> ids)
        {
            return await _dbSet.Where(m => ids.Contains(m.Id)).ToListAsync();
        }
    }
}