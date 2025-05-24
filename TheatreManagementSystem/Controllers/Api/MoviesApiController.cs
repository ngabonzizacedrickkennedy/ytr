using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/movies")]
    public class MoviesApiController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly ITheatreService _theatreService;
        private readonly IScreeningService _screeningService;

        public MoviesApiController(
            IMovieService movieService,
            ITheatreService theatreService,
            IScreeningService screeningService)
        {
            _movieService = movieService;
            _theatreService = theatreService;
            _screeningService = screeningService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MovieDTO>>>> GetAllMovies(
            [FromQuery] string? query = null,
            [FromQuery] string? genre = null,
            [FromQuery] DateTime? date = null)
        {
            try
            {
                List<MovieDTO> movies;

                // Get movies based on filters
                if (!string.IsNullOrEmpty(query))
                {
                    movies = await _movieService.SearchMoviesByTitleAsync(query);
                }
                else if (!string.IsNullOrEmpty(genre))
                {
                    if (Enum.TryParse<MovieGenre>(genre.ToUpper(), out var movieGenre))
                    {
                        movies = await _movieService.GetMoviesByGenreAsync(movieGenre);
                    }
                    else
                    {
                        movies = await _movieService.GetAllMoviesAsync();
                    }
                }
                else
                {
                    movies = await _movieService.GetAllMoviesAsync();
                }

                return Ok(ApiResponse<List<MovieDTO>>.SuccessResult(movies));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MovieDTO>>.ErrorResult($"Error retrieving movies: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MovieDTO>>> GetMovieById(long id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(ApiResponse<MovieDTO>.ErrorResult($"Movie not found with id: {id}"));
                }

                return Ok(ApiResponse<MovieDTO>.SuccessResult(movie));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDTO>.ErrorResult($"Error retrieving movie: {ex.Message}"));
            }
        }

        [HttpGet("{id}/screenings")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, List<ScreeningDTO>>>>> GetMovieScreenings(
            long id,
            [FromQuery] int days = 7)
        {
            try
            {
                // Check if movie exists
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(ApiResponse<Dictionary<string, List<ScreeningDTO>>>.ErrorResult($"Movie not found with id: {id}"));
                }

                // Get screenings for this movie (next X days)
                var screenings = await _screeningService.GetScreeningsByMovieAsync(id, days);

                // Group screenings by date
                var screeningsByDate = screenings
                    .GroupBy(s => s.StartTime.Date)
                    .ToDictionary(
                        g => g.Key.ToString("yyyy-MM-dd"),
                        g => g.OrderBy(s => s.StartTime).ToList()
                    );

                return Ok(ApiResponse<Dictionary<string, List<ScreeningDTO>>>.SuccessResult(screeningsByDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, List<ScreeningDTO>>>.ErrorResult($"Error retrieving movie screenings: {ex.Message}"));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<MovieDTO>>>> SearchMovies([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    return BadRequest(ApiResponse<List<MovieDTO>>.ErrorResult("Search query is required"));
                }

                var movies = await _movieService.SearchMoviesByTitleAsync(query);
                return Ok(ApiResponse<List<MovieDTO>>.SuccessResult(movies));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MovieDTO>>.ErrorResult($"Error searching movies: {ex.Message}"));
            }
        }

        [HttpGet("genre/{genre}")]
        public async Task<ActionResult<ApiResponse<List<MovieDTO>>>> GetMoviesByGenre(string genre)
        {
            try
            {
                if (!Enum.TryParse<MovieGenre>(genre.ToUpper(), out var movieGenre))
                {
                    return BadRequest(ApiResponse<List<MovieDTO>>.ErrorResult($"Invalid genre: {genre}"));
                }

                var movies = await _movieService.GetMoviesByGenreAsync(movieGenre);
                return Ok(ApiResponse<List<MovieDTO>>.SuccessResult(movies));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MovieDTO>>.ErrorResult($"Error retrieving movies by genre: {ex.Message}"));
            }
        }

        [HttpGet("genres")]
        public ActionResult<ApiResponse<List<string>>> GetAllGenres()
        {
            try
            {
                var genres = Enum.GetNames<MovieGenre>().ToList();
                return Ok(ApiResponse<List<string>>.SuccessResult(genres));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Error retrieving genres: {ex.Message}"));
            }
        }

        [HttpGet("ratings")]
        public ActionResult<ApiResponse<List<string>>> GetAllRatings()
        {
            try
            {
                var ratings = Enum.GetNames<MovieRating>().ToList();
                return Ok(ApiResponse<List<string>>.SuccessResult(ratings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<string>>.ErrorResult($"Error retrieving ratings: {ex.Message}"));
            }
        }

        // Admin-only methods
        [HttpPost]
        [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
        public async Task<ActionResult<ApiResponse<MovieDTO>>> CreateMovie([FromBody] MovieDTO movieDTO)
        {
            try
            {
                var createdMovie = await _movieService.CreateMovieAsync(movieDTO);
                return CreatedAtAction(nameof(GetMovieById),
                    new { id = createdMovie.Id },
                    ApiResponse<MovieDTO>.SuccessResult(createdMovie, "Movie created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDTO>.ErrorResult($"Error creating movie: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
        public async Task<ActionResult<ApiResponse<MovieDTO>>> UpdateMovie(long id, [FromBody] MovieDTO movieDTO)
        {
            try
            {
                var updatedMovie = await _movieService.UpdateMovieAsync(id, movieDTO);
                if (updatedMovie == null)
                {
                    return NotFound(ApiResponse<MovieDTO>.ErrorResult($"Movie not found with id: {id}"));
                }

                return Ok(ApiResponse<MovieDTO>.SuccessResult(updatedMovie, "Movie updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDTO>.ErrorResult($"Error updating movie: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ROLE_ADMIN")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMovie(long id)
        {
            try
            {
                // Check if movie exists
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Movie not found with id: {id}"));
                }

                await _movieService.DeleteMovieAsync(id);
                return Ok(ApiResponse<object>.SuccessResult(null, "Movie deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting movie: {ex.Message}"));
            }
        }
    }
}