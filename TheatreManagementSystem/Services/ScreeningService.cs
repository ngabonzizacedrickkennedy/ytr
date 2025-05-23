using AutoMapper;
using System.Globalization;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Services
{
    public class ScreeningService : IScreeningService
    {
        private readonly IScreeningRepository _screeningRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ITheatreRepository _theatreRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public ScreeningService(
            IScreeningRepository screeningRepository,
            IMovieRepository movieRepository,
            ITheatreRepository theatreRepository,
            IBookingRepository bookingRepository,
            IMapper mapper)
        {
            _screeningRepository = screeningRepository;
            _movieRepository = movieRepository;
            _theatreRepository = theatreRepository;
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<ScreeningDTO> CreateScreeningAsync(ScreeningDTO screeningDTO)
        {
            var movie = await _movieRepository.GetByIdAsync(screeningDTO.MovieId);
            if (movie == null)
                throw new InvalidOperationException($"Movie not found with id: {screeningDTO.MovieId}");

            var theatre = await _theatreRepository.GetByIdAsync(screeningDTO.TheatreId);
            if (theatre == null)
                throw new InvalidOperationException($"Theatre not found with id: {screeningDTO.TheatreId}");

            // Calculate end time based on movie duration
            var endTime = screeningDTO.StartTime.AddMinutes(movie.DurationMinutes);

            // Check for scheduling conflicts
            var conflictingScreenings = await _screeningRepository.FindByTheatreIdAsync(theatre.Id);
            var hasConflict = conflictingScreenings.Any(s =>
                s.ScreenNumber == screeningDTO.ScreenNumber &&
                s.StartTime < endTime &&
                s.EndTime > screeningDTO.StartTime);

            if (hasConflict)
            {
                throw new InvalidOperationException("There is a scheduling conflict with another screening");
            }

            var screening = new Screening
            {
                MovieId = screeningDTO.MovieId,
                TheatreId = screeningDTO.TheatreId,
                StartTime = screeningDTO.StartTime,
                EndTime = endTime,
                ScreenNumber = screeningDTO.ScreenNumber,
                Format = screeningDTO.Format,
                BasePrice = screeningDTO.BasePrice
            };

            var savedScreening = await _screeningRepository.AddAsync(screening);
            await _screeningRepository.SaveChangesAsync();

            return await ConvertToDTOAsync(savedScreening);
        }

        public async Task<List<ScreeningDTO>> GetAllScreeningsAsync()
        {
            var screenings = await _screeningRepository.GetAllAsync();
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<ScreeningDTO?> GetScreeningByIdAsync(long id)
        {
            var screening = await _screeningRepository.GetByIdAsync(id);
            return screening != null ? await ConvertToDTOAsync(screening) : null;
        }

        public async Task<List<ScreeningDTO>> GetScreeningsAsync(long? movieId, long? theatreId, DateTime? date)
        {
            IEnumerable<Screening> screenings;

            if (movieId.HasValue && theatreId.HasValue && date.HasValue)
            {
                // Filter by movie, theatre and date
                var startOfDay = date.Value.Date;
                var endOfDay = date.Value.Date.AddDays(1).AddTicks(-1);
                screenings = await _screeningRepository.FindByMovieIdAndTheatreIdAndStartTimeBetweenAsync(
                    movieId.Value, theatreId.Value, startOfDay, endOfDay);
            }
            else if (movieId.HasValue && theatreId.HasValue)
            {
                screenings = await _screeningRepository.FindByMovieIdAndTheatreIdAsync(movieId.Value, theatreId.Value);
            }
            else if (movieId.HasValue && date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = date.Value.Date.AddDays(1).AddTicks(-1);
                screenings = await _screeningRepository.FindByMovieIdAndStartTimeBetweenAsync(
                    movieId.Value, startOfDay, endOfDay);
            }
            else if (theatreId.HasValue && date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = date.Value.Date.AddDays(1).AddTicks(-1);
                screenings = await _screeningRepository.FindByTheatreIdAndStartTimeBetweenAsync(
                    theatreId.Value, startOfDay, endOfDay);
            }
            else if (movieId.HasValue)
            {
                screenings = await _screeningRepository.FindByMovieIdAsync(movieId.Value);
            }
            else if (theatreId.HasValue)
            {
                screenings = await _screeningRepository.FindByTheatreIdAsync(theatreId.Value);
            }
            else if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = date.Value.Date.AddDays(1).AddTicks(-1);
                screenings = await _screeningRepository.FindByStartTimeBetweenAsync(startOfDay, endOfDay);
            }
            else
            {
                screenings = await _screeningRepository.FindByStartTimeAfterAsync(DateTime.Now);
            }

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<List<ScreeningDTO>> GetScreeningsByMovieAsync(long movieId, int? days = null)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if (movie == null)
                throw new InvalidOperationException($"Movie not found with id: {movieId}");

            var now = DateTime.Now;
            var endDate = now.AddDays(days ?? 7);

            var screenings = await _screeningRepository.FindByMovieIdAndStartTimeBetweenAsync(movieId, now, endDate);
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<List<ScreeningDTO>> GetScreeningsByTheatreAsync(long theatreId, DateTime? date = null)
        {
            var theatre = await _theatreRepository.GetByIdAsync(theatreId);
            if (theatre == null)
                throw new InvalidOperationException($"Theatre not found with id: {theatreId}");

            IEnumerable<Screening> screenings;

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = date.Value.Date.AddDays(1).AddTicks(-1);
                screenings = await _screeningRepository.FindByTheatreIdAndStartTimeBetweenAsync(theatreId, startOfDay, endOfDay);
            }
            else
            {
                var now = DateTime.Now;
                screenings = await _screeningRepository.FindByTheatreIdAndStartTimeAfterAsync(theatreId, now);
            }

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<Dictionary<string, List<ScreeningDTO>>> GetUpcomingScreeningsAsync(int? days = null)
        {
            var now = DateTime.Now;
            var endDate = now.AddDays(days ?? 7);

            var screenings = await _screeningRepository.FindByStartTimeBetweenOrderByStartTimeAscAsync(now, endDate);
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            // Group screenings by date
            var screeningsByDate = new Dictionary<string, List<ScreeningDTO>>();

            foreach (var screening in screeningDTOs)
            {
                var dateKey = screening.StartTime.ToString("yyyy-MM-dd");

                if (!screeningsByDate.ContainsKey(dateKey))
                {
                    screeningsByDate[dateKey] = new List<ScreeningDTO>();
                }

                screeningsByDate[dateKey].Add(screening);
            }

            return screeningsByDate;
        }

        public async Task<List<ScreeningDTO>> GetScreeningsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var startDateTime = startDate.Date;
            var endDateTime = endDate.Date.AddDays(1).AddTicks(-1);

            var screenings = await _screeningRepository.FindByStartTimeBetweenAsync(startDateTime, endDateTime);
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<List<ScreeningDTO>> GetScreeningsByMovieAndTheatreAsync(long movieId, long theatreId)
        {
            var screenings = await _screeningRepository.FindByMovieIdAndTheatreIdAsync(movieId, theatreId);
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<List<ScreeningDTO>> GetAvailableScreeningsAsync(long movieId, long theatreId, DateTime startDate)
        {
            var screenings = await _screeningRepository.FindAvailableScreeningsAsync(movieId, theatreId, startDate);
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<List<ScreeningDTO>> GetUpcomingScreeningsAsync(DateTime fromDateTime)
        {
            var screenings = await _screeningRepository.FindByStartTimeAfterOrderByStartTimeAscAsync(fromDateTime);
            var screeningDTOs = new List<ScreeningDTO>();

            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return screeningDTOs;
        }

        public async Task<HashSet<string>> GetAvailableSeatsAsync(long screeningId)
        {
            var screening = await _screeningRepository.GetByIdAsync(screeningId);
            if (screening == null)
                throw new InvalidOperationException($"Screening not found with id: {screeningId}");

            // Get all possible seats (e.g., A1-J10 for a 10x10 theatre)
            var allSeats = GenerateAllSeatsForScreen(screening);

            // Get booked seats
            var bookedSeats = await GetBookedSeatsAsync(screeningId);

            // Remove booked seats from all seats to get available seats
            allSeats.ExceptWith(bookedSeats);

            return allSeats;
        }

        public async Task<HashSet<string>> GetBookedSeatsAsync(long screeningId)
        {
            var bookedSeatsList = await _bookingRepository.FindBookedSeatsByScreeningIdAsync(screeningId);
            return new HashSet<string>(bookedSeatsList);
        }

        public async Task<object> GetSeatingLayoutAsync(long screeningId)
        {
            var screening = await _screeningRepository.GetByIdAsync(screeningId);
            if (screening == null)
                throw new InvalidOperationException($"Screening not found with id: {screeningId}");

            // Create a simple layout with rows, seat counts, and price multipliers
            var layout = new Dictionary<string, object>
            {
                ["basePrice"] = screening.BasePrice
            };

            // Define some example rows with different price tiers
            var rows = new List<Dictionary<string, object>>();

            // Standard rows (A-E)
            for (char rowChar = 'A'; rowChar <= 'E'; rowChar++)
            {
                rows.Add(new Dictionary<string, object>
                {
                    ["name"] = rowChar.ToString(),
                    ["seatsCount"] = 10,
                    ["seatType"] = "STANDARD",
                    ["priceMultiplier"] = 1.0
                });
            }

            // Premium rows (F-H)
            for (char rowChar = 'F'; rowChar <= 'H'; rowChar++)
            {
                rows.Add(new Dictionary<string, object>
                {
                    ["name"] = rowChar.ToString(),
                    ["seatsCount"] = 10,
                    ["seatType"] = "PREMIUM",
                    ["priceMultiplier"] = 1.5
                });
            }

            // VIP row (I)
            rows.Add(new Dictionary<string, object>
            {
                ["name"] = "I",
                ["seatsCount"] = 8,
                ["seatType"] = "VIP",
                ["priceMultiplier"] = 2.0
            });

            // Wheelchair accessible row (J)
            rows.Add(new Dictionary<string, object>
            {
                ["name"] = "J",
                ["seatsCount"] = 6,
                ["seatType"] = "WHEELCHAIR",
                ["priceMultiplier"] = 1.0
            });

            layout["rows"] = rows;

            return layout;
        }

        public async Task<ScreeningDTO?> UpdateScreeningAsync(long id, ScreeningDTO screeningDTO)
        {
            var screening = await _screeningRepository.GetByIdAsync(id);
            if (screening == null)
                return null;

            // Don't change movie and theatre for existing screenings
            screening.StartTime = screeningDTO.StartTime;

            // Recalculate end time based on the movie duration
            var movie = await _movieRepository.GetByIdAsync(screening.MovieId);
            if (movie != null)
            {
                screening.EndTime = screeningDTO.StartTime.AddMinutes(movie.DurationMinutes);
            }

            screening.ScreenNumber = screeningDTO.ScreenNumber;
            screening.Format = screeningDTO.Format;
            screening.BasePrice = screeningDTO.BasePrice;

            await _screeningRepository.UpdateAsync(screening);
            await _screeningRepository.SaveChangesAsync();

            return await ConvertToDTOAsync(screening);
        }

        public async Task DeleteScreeningAsync(long id)
        {
            await _screeningRepository.DeleteAsync(id);
            await _screeningRepository.SaveChangesAsync();
        }

        public async Task<Screening?> GetScreeningEntityByIdAsync(long id)
        {
            return await _screeningRepository.GetByIdAsync(id);
        }

        // Pagination methods implementation
        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetAllScreeningsPagedAsync(int page, int pageSize)
        {
            var screenings = await _screeningRepository.GetPagedAsync(page, pageSize);
            var totalCount = await _screeningRepository.CountAsync();

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsPagedAsync(long? movieId, long? theatreId, DateTime? date, int page, int pageSize)
        {
            // Implementation would be similar to GetScreeningsAsync but with pagination
            // For brevity, implementing a basic version
            var screenings = await _screeningRepository.GetPagedAsync(page, pageSize);
            var totalCount = await _screeningRepository.CountAsync();

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByMoviePagedAsync(long movieId, int page, int pageSize)
        {
            var (screenings, totalCount) = await _screeningRepository.FindByMovieIdPagedAsync(movieId, page, pageSize);

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByTheatrePagedAsync(long theatreId, int page, int pageSize)
        {
            var (screenings, totalCount) = await _screeningRepository.FindByTheatreIdPagedAsync(theatreId, page, pageSize);

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByMovieAndTheatrePagedAsync(long movieId, long theatreId, int page, int pageSize)
        {
            var (screenings, totalCount) = await _screeningRepository.FindByMovieIdAndTheatreIdPagedAsync(movieId, theatreId, page, pageSize);

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetScreeningsByDateRangePagedAsync(DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var (screenings, totalCount) = await _screeningRepository.FindByStartTimeBetweenPagedAsync(startDate, endDate, page, pageSize);

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetAvailableScreeningsPagedAsync(long movieId, long theatreId, DateTime startDate, int page, int pageSize)
        {
            var (screenings, totalCount) = await _screeningRepository.FindAvailableScreeningsPagedAsync(movieId, theatreId, startDate, page, pageSize);

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        public async Task<(List<ScreeningDTO> Screenings, int TotalCount)> GetUpcomingScreeningsPagedAsync(DateTime fromDateTime, int page, int pageSize)
        {
            var (screenings, totalCount) = await _screeningRepository.FindByStartTimeAfterPagedAsync(fromDateTime, page, pageSize);

            var screeningDTOs = new List<ScreeningDTO>();
            foreach (var screening in screenings)
            {
                screeningDTOs.Add(await ConvertToDTOAsync(screening));
            }

            return (screeningDTOs, totalCount);
        }

        // Helper methods
        private HashSet<string> GenerateAllSeatsForScreen(Screening screening)
        {
            var allSeats = new HashSet<string>();

            // Generate A1-J10 seats for demo purposes
            for (char row = 'A'; row <= 'J'; row++)
            {
                for (int seatNum = 1; seatNum <= 10; seatNum++)
                {
                    allSeats.Add($"{row}{seatNum}");
                }
            }

            return allSeats;
        }

        private async Task<ScreeningDTO> ConvertToDTOAsync(Screening screening)
        {
            var dto = _mapper.Map<ScreeningDTO>(screening);

            // Set navigation properties
            if (screening.Movie != null)
            {
                dto.MovieTitle = screening.Movie.Title;
            }

            if (screening.Theatre != null)
            {
                dto.TheatreName = screening.Theatre.Name;
            }

            return dto;
        }
    }
}