using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Repositories.Interfaces
{
    public interface ISeatRepository : IGenericRepository<Seat>
    {
        Task<IEnumerable<Seat>> FindByTheatreIdAndScreenNumberAsync(long theatreId, int screenNumber);
        Task<IEnumerable<Seat>> FindByTheatreIdAndScreenNumberAndSeatTypeAsync(long theatreId, int screenNumber, SeatType seatType);
        Task<IEnumerable<Seat>> FindByTheatreIdAndScreenNumberAndRowNameAsync(long theatreId, int screenNumber, string rowName);
    }
}