using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Services.Interfaces
{
    public interface ISeatService
    {
        Task<Seat?> GetSeatByIdAsync(long id);
        Task InitializeSeatsForTheatreAsync(long theatreId, int screenNumber, int rows, int seatsPerRow);
        Task<List<Seat>> GetSeatsByTheatreAndScreenAsync(long theatreId, int screenNumber);
        Task<Dictionary<string, List<Seat>>> GetSeatMapByTheatreAndScreenAsync(long theatreId, int screenNumber);
        Task<List<Seat>> GetSeatsByTypeAsync(long theatreId, int screenNumber, SeatType seatType);
        Task UpdateSeatTypeAsync(long seatId, SeatType seatType, double priceMultiplier);
        Task UpdateSeatRowTypeAsync(long theatreId, int screenNumber, string rowName, SeatType seatType, double priceMultiplier);
        Task<int> BulkUpdateSeatsAsync(List<string> seatIds, long theatreId, int screenNumber, SeatType seatType, double priceMultiplier);
        Task<int> DeleteScreenSeatsAsync(long theatreId, int screenNumber);
    }
}