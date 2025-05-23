using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Data;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Repositories
{
    public class ScreeningRepository : GenericRepository<Screening>, IScreeningRepository
    {
        public ScreeningRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Screening>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .ToListAsync();
        }

        public override async Task<Screening?> GetByIdAsync(long id)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Screening>> FindByMovieIdAsync(long movieId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByTheatreIdAsync(long theatreId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByMovieIdAndTheatreIdAsync(long movieId, long theatreId)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.TheatreId == theatreId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByStartTimeAfterAndStartTimeBeforeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime > startDate && s.StartTime < endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindAvailableScreeningsAsync(long movieId, long theatreId, DateTime startDate)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.TheatreId == theatreId && s.StartTime >= startDate)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<long> CountBookingsByScreeningIdAsync(long screeningId)
        {
            return await _context.Bookings.CountAsync(b => b.ScreeningId == screeningId);
        }

        public async Task<IEnumerable<Screening>> FindByStartTimeAfterOrderByStartTimeAscAsync(DateTime startTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime > startTime)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByStartTimeBetweenOrderByStartTimeAscAsync(DateTime startTime, DateTime endTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime >= startTime && s.StartTime <= endTime)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByStartTimeBetweenAsync(DateTime startTime, DateTime endTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime >= startTime && s.StartTime <= endTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByTheatreIdAndStartTimeAfterAsync(long theatreId, DateTime startTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.StartTime > startTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByTheatreIdAndStartTimeBetweenAsync(long theatreId, DateTime startTime, DateTime endTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.StartTime >= startTime && s.StartTime <= endTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByMovieIdAndStartTimeBetweenAsync(long movieId, DateTime startTime, DateTime endTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.StartTime >= startTime && s.StartTime <= endTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByMovieIdAndTheatreIdAndStartTimeBetweenAsync(long movieId, long theatreId, DateTime startTime, DateTime endTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.TheatreId == theatreId && s.StartTime >= startTime && s.StartTime <= endTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> FindByStartTimeAfterAsync(DateTime startTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime > startTime)
                .ToListAsync();
        }

        // Pagination implementations
        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdPagedAsync(long movieId, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByTheatreIdPagedAsync(long theatreId, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdAndTheatreIdPagedAsync(long movieId, long theatreId, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.TheatreId == theatreId);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByStartTimeAfterAndStartTimeBeforePagedAsync(DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime > startDate && s.StartTime < endDate);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByStartTimeAfterPagedAsync(DateTime startTime, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime > startTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByStartTimeBetweenPagedAsync(DateTime startTime, DateTime endTime, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.StartTime >= startTime && s.StartTime <= endTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByTheatreIdAndStartTimeAfterPagedAsync(long theatreId, DateTime startTime, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.StartTime > startTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByTheatreIdAndStartTimeBetweenPagedAsync(long theatreId, DateTime startTime, DateTime endTime, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.StartTime >= startTime && s.StartTime <= endTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdAndStartTimeBetweenPagedAsync(long movieId, DateTime startTime, DateTime endTime, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.StartTime >= startTime && s.StartTime <= endTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdAndTheatreIdAndStartTimeBetweenPagedAsync(long movieId, long theatreId, DateTime startTime, DateTime endTime, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.TheatreId == theatreId && s.StartTime >= startTime && s.StartTime <= endTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        public async Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindAvailableScreeningsPagedAsync(long movieId, long theatreId, DateTime startDate, int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s => s.MovieId == movieId && s.TheatreId == theatreId && s.StartTime >= startDate)
                .OrderBy(s => s.StartTime);

            var totalCount = await query.CountAsync();
            var screenings = await query
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (screenings, totalCount);
        }

        // Search methods
        public async Task<IEnumerable<Screening>> FindByMovieTitleOrTheatreNameAndStartTimeAfterAsync(string movieTitle, string theatreName, DateTime startTime)
        {
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s =>
                    (s.Movie.Title.ToLower().Contains(movieTitle.ToLower()) ||
                     s.Theatre.Name.ToLower().Contains(theatreName.ToLower())) &&
                    s.StartTime > startTime)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Screening>> SearchScreeningsAsync(string query, int limit = 10)
        {
            var searchTerm = query.ToLower();
            return await _dbSet
                .Include(s => s.Movie)
                .Include(s => s.Theatre)
                .Where(s =>
                    s.Movie.Title.ToLower().Contains(searchTerm) ||
                    s.Theatre.Name.ToLower().Contains(searchTerm) ||
                    (s.Movie.Director != null && s.Movie.Director.ToLower().Contains(searchTerm)))
                .OrderBy(s => s.StartTime)
                .Take(limit)
                .ToListAsync();
        }
    }
}