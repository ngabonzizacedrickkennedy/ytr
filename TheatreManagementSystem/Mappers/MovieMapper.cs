using AutoMapper;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Mappers
{
    /// <summary>
    /// Dedicated mapper class for Movie entities (matching Spring Boot MovieMapper)
    /// This provides fine-grained control over movie mapping when AutoMapper profiles aren't sufficient
    /// </summary>
    public class MovieMapper
    {
        private readonly IMapper _mapper;

        public MovieMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Convert Movie entity to MovieDTO
        /// </summary>
        /// <param name="movie">Movie entity</param>
        /// <returns>MovieDTO</returns>
        public MovieDTO ToDTO(Movie movie)
        {
            if (movie == null)
                return null!;

            return _mapper.Map<MovieDTO>(movie);
        }

        /// <summary>
        /// Convert MovieDTO to Movie entity
        /// </summary>
        /// <param name="movieDTO">MovieDTO</param>
        /// <returns>Movie entity</returns>
        public Movie ToEntity(MovieDTO movieDTO)
        {
            if (movieDTO == null)
                return null!;

            return _mapper.Map<Movie>(movieDTO);
        }

        /// <summary>
        /// Update existing Movie entity with data from MovieDTO
        /// </summary>
        /// <param name="movieDTO">Source DTO</param>
        /// <param name="movie">Target entity to update</param>
        public void UpdateEntityFromDTO(MovieDTO movieDTO, Movie movie)
        {
            if (movieDTO == null || movie == null)
                return;

            // Map properties while preserving the ID and navigation properties
            var originalId = movie.Id;
            var originalScreenings = movie.Screenings;

            _mapper.Map(movieDTO, movie);

            // Restore preserved properties
            movie.Id = originalId;
            movie.Screenings = originalScreenings;
        }

        /// <summary>
        /// Convert a collection of Movie entities to MovieDTOs
        /// </summary>
        /// <param name="movies">Collection of Movie entities</param>
        /// <returns>Collection of MovieDTOs</returns>
        public IEnumerable<MovieDTO> ToDTO(IEnumerable<Movie> movies)
        {
            return movies?.Select(ToDTO) ?? Enumerable.Empty<MovieDTO>();
        }

        /// <summary>
        /// Convert a collection of MovieDTOs to Movie entities
        /// </summary>
        /// <param name="movieDTOs">Collection of MovieDTOs</param>
        /// <returns>Collection of Movie entities</returns>
        public IEnumerable<Movie> ToEntity(IEnumerable<MovieDTO> movieDTOs)
        {
            return movieDTOs?.Select(ToEntity) ?? Enumerable.Empty<Movie>();
        }
    }
}