using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Services
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;
        private readonly ITheatreRepository _theatreRepository;

        public SeatService(ISeatRepository seatRepository, ITheatreRepository theatreRepository)
        {
            _seatRepository = seatRepository;
            _theatreRepository = theatreRepository;
        }

        public async Task<Seat?> GetSeatByIdAsync(long id)
        {
            return await _seatRepository.GetByIdAsync(id);
        }

        public async Task InitializeSeatsForTheatreAsync(long theatreId, int screenNumber, int rows, int seatsPerRow)
        {
            var theatre = await _theatreRepository.GetByIdAsync(theatreId);
            if (theatre == null)
                throw new InvalidOperationException($"Theatre not found with id: {theatreId}");

            // Check if seats already exist for this screen
            var existingSeats = await _seatRepository.FindByTheatreIdAndScreenNumberAsync(theatreId, screenNumber);
            if (existingSeats.Any())
            {
                throw new InvalidOperationException("Seats already exist for this screen. Delete them first before reinitializing.");
            }

            var seats = new List<Seat>();

            for (int row = 0; row < rows; row++)
            {
                // Convert row number to letter (A, B, C, ...)
                string rowName = ((char)('A' + row)).ToString();

                for (int seatNum = 1; seatNum <= seatsPerRow; seatNum++)
                {
                    var seat = new Seat
                    {
                        TheatreId = theatreId,
                        ScreenNumber = screenNumber,
                        RowName = rowName,
                        SeatNumber = seatNum
                    };

                    // Assign seat types based on position
                    if (row < 2)
                    {
                        // Front rows are standard
                        seat.SeatType = SeatType.STANDARD;
                        seat.PriceMultiplier = 1.0;
                    }
                    else if (row >= 2 && row < rows - 2)
                    {
                        // Middle rows are premium
                        seat.SeatType = SeatType.PREMIUM;
                        seat.PriceMultiplier = 1.2;
                    }
                    else
                    {
                        // Back rows are VIP
                        seat.SeatType = SeatType.VIP;
                        seat.PriceMultiplier = 1.5;
                    }

                    // Mark some seats as accessible
                    if (row == rows / 2 && (seatNum == 1 || seatNum == seatsPerRow))
                    {
                        seat.SeatType = SeatType.ACCESSIBLE;
                        seat.PriceMultiplier = 1.0;
                    }

                    seats.Add(seat);
                }
            }

            await _seatRepository.AddRangeAsync(seats);
            await _seatRepository.SaveChangesAsync();
        }

        public async Task<List<Seat>> GetSeatsByTheatreAndScreenAsync(long theatreId, int screenNumber)
        {
            return (await _seatRepository.FindByTheatreIdAndScreenNumberAsync(theatreId, screenNumber)).ToList();
        }

        public async Task<Dictionary<string, List<Seat>>> GetSeatMapByTheatreAndScreenAsync(long theatreId, int screenNumber)
        {
            // Group seats by row for easier display
            var seats = await _seatRepository.FindByTheatreIdAndScreenNumberAsync(theatreId, screenNumber);

            var seatsByRow = seats
                .GroupBy(s => s.RowName)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(s => s.SeatNumber).ToList()
                );

            // Sort by row name
            return seatsByRow
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<List<Seat>> GetSeatsByTypeAsync(long theatreId, int screenNumber, SeatType seatType)
        {
            return (await _seatRepository.FindByTheatreIdAndScreenNumberAndSeatTypeAsync(theatreId, screenNumber, seatType)).ToList();
        }

        public async Task UpdateSeatTypeAsync(long seatId, SeatType seatType, double priceMultiplier)
        {
            var seat = await _seatRepository.GetByIdAsync(seatId);
            if (seat == null)
                return;

            seat.SeatType = seatType;
            seat.PriceMultiplier = priceMultiplier;
            await _seatRepository.UpdateAsync(seat);
            await _seatRepository.SaveChangesAsync();
        }

        public async Task UpdateSeatRowTypeAsync(long theatreId, int screenNumber, string rowName, SeatType seatType, double priceMultiplier)
        {
            var rowSeats = await _seatRepository.FindByTheatreIdAndScreenNumberAndRowNameAsync(theatreId, screenNumber, rowName);

            if (!rowSeats.Any())
            {
                throw new InvalidOperationException("No seats found for the specified row");
            }

            foreach (var seat in rowSeats)
            {
                seat.SeatType = seatType;
                seat.PriceMultiplier = priceMultiplier;
            }

            await _seatRepository.UpdateRangeAsync(rowSeats);
            await _seatRepository.SaveChangesAsync();
        }

        public async Task<int> BulkUpdateSeatsAsync(List<string> seatIds, long theatreId, int screenNumber, SeatType seatType, double priceMultiplier)
        {
            var seats = await _seatRepository.FindByTheatreIdAndScreenNumberAsync(theatreId, screenNumber);

            var seatsToUpdate = seats
                .Where(seat => seatIds.Contains($"{seat.RowName}{seat.SeatNumber}"))
                .ToList();

            foreach (var seat in seatsToUpdate)
            {
                seat.SeatType = seatType;
                seat.PriceMultiplier = priceMultiplier;
            }

            await _seatRepository.UpdateRangeAsync(seatsToUpdate);
            await _seatRepository.SaveChangesAsync();

            return seatsToUpdate.Count;
        }

        public async Task<int> DeleteScreenSeatsAsync(long theatreId, int screenNumber)
        {
            var seats = await _seatRepository.FindByTheatreIdAndScreenNumberAsync(theatreId, screenNumber);
            var count = seats.Count();

            await _seatRepository.DeleteRangeAsync(seats);
            await _seatRepository.SaveChangesAsync();

            return count;
        }
    }
}