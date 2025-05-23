using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Data;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Repositories
{
    public class SeatRepository : GenericRepository<Seat>, ISeatRepository
    {
        public SeatRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Seat>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Theatre)
                .ToListAsync();
        }

        public override async Task<Seat?> GetByIdAsync(long id)
        {
            return await _dbSet
                .Include(s => s.Theatre)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Seat>> FindByTheatreIdAndScreenNumberAsync(long theatreId, int screenNumber)
        {
            return await _dbSet
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.ScreenNumber == screenNumber)
                .OrderBy(s => s.RowName)
                .ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> FindByTheatreIdAndScreenNumberAndSeatTypeAsync(long theatreId, int screenNumber, SeatType seatType)
        {
            return await _dbSet
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.ScreenNumber == screenNumber && s.SeatType == seatType)
                .OrderBy(s => s.RowName)
                .ThenBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Seat>> FindByTheatreIdAndScreenNumberAndRowNameAsync(long theatreId, int screenNumber, string rowName)
        {
            return await _dbSet
                .Include(s => s.Theatre)
                .Where(s => s.TheatreId == theatreId && s.ScreenNumber == screenNumber && s.RowName == rowName)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }
    }
}