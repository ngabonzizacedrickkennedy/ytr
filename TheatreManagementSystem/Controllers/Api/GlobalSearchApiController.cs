using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Services.Interfaces;

namespace TheatreManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/search")]
    public class GlobalSearchApiController : ControllerBase
    {
        private readonly IGlobalSearchService _globalSearchService;

        public GlobalSearchApiController(IGlobalSearchService globalSearchService)
        {
            _globalSearchService = globalSearchService;
        }

        /// <summary>
        /// Global search across all entities
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GlobalSearch(
            [FromQuery] string query,
            [FromQuery] int limit = 3)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Search query is required"));
                }

                var results = await _globalSearchService.GlobalSearchAsync(query, limit);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResult(results));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Error performing global search: {ex.Message}"));
            }
        }

        /// <summary>
        /// Search movies only
        /// </summary>
        [HttpGet("movies")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> SearchMovies(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Search query is required"));
                }

                var results = await _globalSearchService.SearchMoviesAsync(query, limit);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResult(results));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Error searching movies: {ex.Message}"));
            }
        }

        /// <summary>
        /// Search theatres only
        /// </summary>
        [HttpGet("theatres")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> SearchTheatres(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Search query is required"));
                }

                var results = await _globalSearchService.SearchTheatresAsync(query, limit);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResult(results));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Error searching theatres: {ex.Message}"));
            }
        }

        /// <summary>
        /// Search screenings only
        /// </summary>
        [HttpGet("screenings")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> SearchScreenings(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Search query is required"));
                }

                var results = await _globalSearchService.SearchScreeningsAsync(query, limit);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResult(results));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Error searching screenings: {ex.Message}"));
            }
        }

        /// <summary>
        /// Search users only (Admin only)
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = "ROLE_ADMIN")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> SearchUsers(
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Search query is required"));
                }

                var results = await _globalSearchService.SearchUsersAsync(query, limit);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResult(results));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Error searching users: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get search suggestions
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetSearchSuggestions(
            [FromQuery] string query,
            [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult("Search query is required"));
                }

                var suggestions = await _globalSearchService.GetSearchSuggestionsAsync(query, limit);
                return Ok(ApiResponse<Dictionary<string, object>>.SuccessResult(suggestions));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<Dictionary<string, object>>.ErrorResult($"Error retrieving search suggestions: {ex.Message}"));
            }
        }
    }
}