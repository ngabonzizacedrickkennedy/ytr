using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IMapper _mapper;

        public MovieService(IMovieRepository movieRepository, IMapper mapper)
        {
            _movieRepository = movieRepository;
            _mapper = mapper;
        }

        public async Task<List<MovieDTO>> GetAllMoviesAsync()
        {
            var movies = await _movieRepository.GetAllAsync();
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        public async Task<MovieDTO?> GetMovieByIdAsync(long id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            return movie != null ? _mapper.Map<MovieDTO>(movie) : null;
        }

        public async Task<MovieDTO> CreateMovieAsync(MovieDTO movieDTO)
        {
            var movie = _mapper.Map<Movie>(movieDTO);
            var savedMovie = await _movieRepository.AddAsync(movie);
            await _movieRepository.SaveChangesAsync();
            return _mapper.Map<MovieDTO>(savedMovie);
        }

        public async Task<MovieDTO?> UpdateMovieAsync(long id, MovieDTO movieDTO)
        {
            var existingMovie = await _movieRepository.GetByIdAsync(id);
            if (existingMovie == null)
                return null;

            // Update movie properties
            _mapper.Map(movieDTO, existingMovie);
            existingMovie.Id = id; // Ensure ID is preserved

            await _movieRepository.UpdateAsync(existingMovie);
            await _movieRepository.SaveChangesAsync();

            return _mapper.Map<MovieDTO>(existingMovie);
        }

        public async Task DeleteMovieAsync(long id)
        {
            await _movieRepository.DeleteAsync(id);
            await _movieRepository.SaveChangesAsync();
        }

        public async Task<List<MovieDTO>> SearchMoviesByTitleAsync(string title)
        {
            var movies = await _movieRepository.FindByTitleContainingIgnoreCaseAsync(title);
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        public async Task<List<MovieDTO>> GetMoviesByGenreAsync(MovieGenre genre)
        {
            var movies = await _movieRepository.FindByGenreAsync(genre);
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        public async Task<List<MovieDTO>> GetUpcomingMoviesAsync()
        {
            var currentDate = DateTime.Now;
            var movies = await _movieRepository.FindUpcomingMoviesAsync(currentDate);
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        public async Task<List<MovieDTO>> GetMoviesByIdsAsync(IEnumerable<long> ids)
        {
            var movies = await _movieRepository.FindByIdsAsync(ids);
            return _mapper.Map<List<MovieDTO>>(movies);
        }

        // Pagination methods
        public async Task<(List<MovieDTO> Movies, int TotalCount)> GetAllMoviesPagedAsync(int page, int pageSize)
        {
            var movies = await _movieRepository.GetPagedAsync(page, pageSize);
            var totalCount = await _movieRepository.CountAsync();
            return (_mapper.Map<List<MovieDTO>>(movies), totalCount);
        }

        public async Task<(List<MovieDTO> Movies, int TotalCount)> SearchMoviesByTitlePagedAsync(string title, int page, int pageSize)
        {
            var (movies, totalCount) = await _movieRepository.FindByTitleContainingIgnoreCasePagedAsync(title, page, pageSize);
            return (_mapper.Map<List<MovieDTO>>(movies), totalCount);
        }

        public async Task<(List<MovieDTO> Movies, int TotalCount)> GetMoviesByGenrePagedAsync(MovieGenre genre, int page, int pageSize)
        {
            var (movies, totalCount) = await _movieRepository.FindByGenrePagedAsync(genre, page, pageSize);
            return (_mapper.Map<List<MovieDTO>>(movies), totalCount);
        }

        public async Task<(List<MovieDTO> Movies, int TotalCount)> GetUpcomingMoviesPagedAsync(int page, int pageSize)
        {
            var currentDate = DateTime.Now;
            var (movies, totalCount) = await _movieRepository.FindUpcomingMoviesPagedAsync(currentDate, page, pageSize);
            return (_mapper.Map<List<MovieDTO>>(movies), totalCount);
        }

        public async Task<(List<MovieDTO> Movies, int TotalCount)> GetMoviesWithFiltersPagedAsync(string? title = null, MovieGenre? genre = null, int page = 0, int pageSize = 10)
        {
            var (movies, totalCount) = await _movieRepository.FindMoviesWithFiltersPagedAsync(title, genre, page, pageSize);
            return (_mapper.Map<List<MovieDTO>>(movies), totalCount);
        }
    }
}