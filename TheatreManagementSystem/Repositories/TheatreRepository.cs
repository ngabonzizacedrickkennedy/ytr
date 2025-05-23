using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Data;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;

namespace TheatreManagementSystem.Repositories
{
    public class TheatreRepository : GenericRepository<Theatre>, ITheatreRepository
    {
        public TheatreRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Theatre>> FindByNameContainingIgnoreCaseAsync(string name)
        {
            return await _dbSet
                .Where(t => t.Name.ToLower().Contains(name.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Theatre>> FindByAddressContainingIgnoreCaseAsync(string address)
        {
            return await _dbSet
                .Where(t => t.Address.ToLower().Contains(address.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Theatre>> FindTheatresByMovieIdAsync(long movieId)
        {
            return await _context.Theatres
                .Where(t => t.Screenings.Any(s => s.MovieId == movieId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Theatre>> FindByNameContainingIgnoreCaseOrAddressContainingIgnoreCaseAsync(string name, string address)
        {
            return await _dbSet
                .Where(t =>
                    t.Name.ToLower().Contains(name.ToLower()) ||
                    t.Address.ToLower().Contains(address.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Theatre>> SearchTheatresAsync(string query)
        {
            var searchTerm = query.ToLower();
            return await _dbSet
                .Where(t =>
                    t.Name.ToLower().Contains(searchTerm) ||
                    t.Address.ToLower().Contains(searchTerm) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)))
                .ToListAsync();
        }
    }
}