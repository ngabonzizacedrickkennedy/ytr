using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IScreeningService
    {
        // Basic CRUD operations
        Task<ScreeningDTO> CreateScreeningAsync(ScreeningDTO screeningDTO);
        Task<List<ScreeningDTO>> GetAllScreeningsAsync();
        Task<ScreeningDTO?> GetScreeningByIdAsync(long id);
        Task<ScreeningDTO?> UpdateScreeningAsync(long id, ScreeningDTO screeningDTO);
        Task DeleteScreeningAsync(long id);

        // Query methods with optional filtering
        Task<List<ScreeningDTO>> GetScreeningsAsync(long? movieId, long? theatreId, DateTime? date);
        Task<List<ScreeningDTO>> GetScreeningsByMovieAsync(long movieId, int? days = null);
        Task<List<ScreeningDTO>> GetScreeningsByTheatreAsync(long theatreId, DateTime? date = null);
        Task<Dictionary<string, List<ScreeningDTO>>> GetUpcomingScreeningsAsync(int? days = null);
        Task<List<ScreeningDTO>> GetScreeningsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<ScreeningDTO>> GetScreeningsByMovieAndTheatreAsync(long movieId, long theatreId);
        Task<List<ScreeningDTO>> GetAvailableScreeningsAsync(long movieId, long theatreId, DateTime startDate);
        Task<List<ScreeningDTO>> GetUpcomingScreeningsAsync(DateTime fromDateTime);

        // Seat management
        Task<HashSet<string>> GetAvailableSeatsAsync(long screeningId);
        Task<HashSet<string>> GetBookedSeatsAsync(long screeningId);
        Task<object> GetSeatingLayoutAsync(long screeningId);

        // Pagination methods
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetAllScreeningsPagedAsync(int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsPagedAsync(long? movieId, long? theatreId, DateTime? date, int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByMoviePagedAsync(long movieId, int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByTheatrePagedAsync(long theatreId, int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByMovieAndTheatrePagedAsync(long movieId, long theatreId, int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByDateRangePagedAsync(DateTime startDate, DateTime endDate, int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetAvailableScreeningsPagedAsync(long movieId, long theatreId, DateTime startDate, int page, int pageSize);
        Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetUpcomingScreeningsPagedAsync(DateTime fromDateTime, int page, int pageSize);

        // Entity access
        Task<Screening?> GetScreeningEntityByIdAsync(long id);
    }
}