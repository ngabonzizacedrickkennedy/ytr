using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Repositories.Interfaces
{
    public interface IScreeningRepository : IGenericRepository<Screening>
    {
        Task<IEnumerable<Screening>> FindByMovieIdAsync(long movieId);
        Task<IEnumerable<Screening>> FindByTheatreIdAsync(long theatreId);
        Task<IEnumerable<Screening>> FindByMovieIdAndTheatreIdAsync(long movieId, long theatreId);
        Task<IEnumerable<Screening>> FindByStartTimeAfterAndStartTimeBeforeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Screening>> FindAvailableScreeningsAsync(long movieId, long theatreId, DateTime startDate);
        Task<long> CountBookingsByScreeningIdAsync(long screeningId);
        Task<IEnumerable<Screening>> FindByStartTimeAfterOrderByStartTimeAscAsync(DateTime startTime);
        Task<IEnumerable<Screening>> FindByStartTimeBetweenOrderByStartTimeAscAsync(DateTime startTime, DateTime endTime);
        Task<IEnumerable<Screening>> FindByStartTimeBetweenAsync(DateTime startTime, DateTime endTime);
        Task<IEnumerable<Screening>> FindByTheatreIdAndStartTimeAfterAsync(long theatreId, DateTime startTime);
        Task<IEnumerable<Screening>> FindByTheatreIdAndStartTimeBetweenAsync(long theatreId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<Screening>> FindByMovieIdAndStartTimeBetweenAsync(long movieId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<Screening>> FindByMovieIdAndTheatreIdAndStartTimeBetweenAsync(long movieId, long theatreId, DateTime startTime, DateTime endTime);
        Task<IEnumerable<Screening>> FindByStartTimeAfterAsync(DateTime startTime);

        // Pagination versions
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdPagedAsync(long movieId, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByTheatreIdPagedAsync(long theatreId, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdAndTheatreIdPagedAsync(long movieId, long theatreId, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByStartTimeAfterAndStartTimeBeforePagedAsync(DateTime startDate, DateTime endDate, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByStartTimeAfterPagedAsync(DateTime startTime, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByStartTimeBetweenPagedAsync(DateTime startTime, DateTime endTime, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByTheatreIdAndStartTimeAfterPagedAsync(long theatreId, DateTime startTime, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByTheatreIdAndStartTimeBetweenPagedAsync(long theatreId, DateTime startTime, DateTime endTime, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdAndStartTimeBetweenPagedAsync(long movieId, DateTime startTime, DateTime endTime, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindByMovieIdAndTheatreIdAndStartTimeBetweenPagedAsync(long movieId, long theatreId, DateTime startTime, DateTime endTime, int page, int pageSize);
        Task<(IEnumerable<Screening> Screenings, int TotalCount)> FindAvailableScreeningsPagedAsync(long movieId, long theatreId, DateTime startDate, int page, int pageSize);

        // New search methods for global search
        Task<IEnumerable<Screening>> FindByMovieTitleOrTheatreNameAndStartTimeAfterAsync(string movieTitle, string theatreName, DateTime startTime);
        Task<IEnumerable<Screening>> SearchScreeningsAsync(string query, int limit = 10);
    }
}