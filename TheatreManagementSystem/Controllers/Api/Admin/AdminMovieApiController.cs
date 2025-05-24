using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api.Admin
{
    [ApiController]
    [Route("api/admin/movies")]
    [Authorize(Roles = "ROLE_ADMIN,ROLE_MANAGER")]
    public class AdminMovieApiController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IScreeningService _screeningService;
        private readonly ITheatreService _theatreService;

        public AdminMovieApiController(
            IMovieService movieService,
            IScreeningService screeningService,
            ITheatreService theatreService)
        {
            _movieService = movieService;
            _screeningService = screeningService;
            _theatreService = theatreService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<AdminMovieListResponse>>> GetMovies(
            [FromQuery] string? search = null,
            [FromQuery] string? genre = null,
            [FromQuery] string sortBy = "title",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            try
            {
                // Validate page and size parameters
                if (page < 0) page = 0;
                if (size < 1) size = 10;
                if (size > 100) size = 100; // Limit maximum page size

                // Get movies based on filters (using paginated version)
                (List<MovieDTO> movies, int totalCount) result;

                if (!string.IsNullOrEmpty(search))
                {
                    result = await _movieService.SearchMoviesByTitlePagedAsync(search.Trim(), page, size);
                }
                else if (!string.IsNullOrEmpty(genre))
                {
                    if (Enum.TryParse<MovieGenre>(genre.ToUpper(), out var genreEnum))
                    {
                        result = await _movieService.GetMoviesByGenrePagedAsync(genreEnum, page, size);
                    }
                    else
                    {
                        result = await _movieService.GetAllMoviesPagedAsync(page, size);
                    }
                }
                else
                {
                    result = await _movieService.GetAllMoviesPagedAsync(page, size);
                }

                // Apply sorting (simple in-memory sorting for now)
                var sortedMovies = sortBy.ToLower() switch
                {
                    "title" => sortOrder.ToLower() == "desc"
                        ? result.movies.OrderByDescending(m => m.Title).ToList()
                        : result.movies.OrderBy(m => m.Title).ToList(),
                    "genre" => sortOrder.ToLower() == "desc"
                        ? result.movies.OrderByDescending(m => m.Genre).ToList()
                        : result.movies.OrderBy(m => m.Genre).ToList(),
                    "releasedate" => sortOrder.ToLower() == "desc"
                        ? result.movies.OrderByDescending(m => m.ReleaseDate).ToList()
                        : result.movies.OrderBy(m => m.ReleaseDate).ToList(),
                    "rating" => sortOrder.ToLower() == "desc"
                        ? result.movies.OrderByDescending(m => m.Rating).ToList()
                        : result.movies.OrderBy(m => m.Rating).ToList(),
                    "durationminutes" => sortOrder.ToLower() == "desc"
                        ? result.movies.OrderByDescending(m => m.DurationMinutes).ToList()
                        : result.movies.OrderBy(m => m.DurationMinutes).ToList(),
                    _ => result.movies.OrderBy(m => m.Title).ToList()
                };

                // Prepare response with pagination metadata
                var response = new AdminMovieListResponse
                {
                    Movies = sortedMovies,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling((double)result.totalCount / size),
                    TotalElements = result.totalCount,
                    PageSize = size,
                    HasNext = (page + 1) * size < result.totalCount,
                    HasPrevious = page > 0,
                    IsFirst = page == 0,
                    IsLast = (page + 1) * size >= result.totalCount
                };

                return Ok(ApiResponse<AdminMovieListResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminMovieListResponse>.ErrorResult($"Error retrieving movies: {ex.Message}"));
            }
        }

        [HttpGet("genres")]
        public ActionResult<ApiResponse<List<string>>> GetGenres()
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
        public ActionResult<ApiResponse<List<string>>> GetRatings()
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MovieDTO>>> CreateMovie([FromBody] MovieDTO movieDTO)
        {
            try
            {
                var createdMovie = await _movieService.CreateMovieAsync(movieDTO);
                return CreatedAtAction(nameof(GetMovie),
                    new { id = createdMovie.Id },
                    ApiResponse<MovieDTO>.SuccessResult(createdMovie, "Movie created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDTO>.ErrorResult($"Error creating movie: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MovieDTO>>> GetMovie(long id)
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

        [HttpPut("{id}")]
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

        [HttpGet("{id}/screenings")]
        public async Task<ActionResult<ApiResponse<AdminMovieScreeningsResponse>>> GetMovieScreenings(long id)
        {
            try
            {
                var movie = await _movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return NotFound(ApiResponse<AdminMovieScreeningsResponse>.ErrorResult($"Movie not found with id: {id}"));
                }

                var screenings = await _screeningService.GetScreeningsByMovieAsync(id);
                var theatres = await _theatreService.GetTheatresByMovieAsync(id);

                var response = new AdminMovieScreeningsResponse
                {
                    Movie = movie,
                    Screenings = screenings,
                    Theatres = theatres
                };

                return Ok(ApiResponse<AdminMovieScreeningsResponse>.SuccessResult(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<AdminMovieScreeningsResponse>.ErrorResult($"Error retrieving movie screenings: {ex.Message}"));
            }
        }
    }

    public class AdminMovieListResponse
    {
        public List<MovieDTO> Movies { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalElements { get; set; }
        public int PageSize { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
    }

    public class AdminMovieScreeningsResponse
    {
        public MovieDTO Movie { get; set; } = new();
        public List<ScreeningDTO> Screenings { get; set; } = new();
        public List<TheatreDTO> Theatres { get; set; } = new();
    }
}