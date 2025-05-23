using Microsoft.Extensions.Caching.Memory;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Models;
using TheatreManagementSystem.Repositories.Interfaces;
using TheatreManagementSystem.Services.Interfaces;
using AutoMapper;
namespace TheatreManagementSystem.Services
{
    public class GlobalSearchService : IGlobalSearchService
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ITheatreRepository _theatreRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GlobalSearchService(
            IMovieRepository movieRepository,
            ITheatreRepository theatreRepository,
            IScreeningRepository screeningRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _movieRepository = movieRepository;
            _theatreRepository = theatreRepository;
            _screeningRepository = screeningRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Dictionary<string, object>> GlobalSearchAsync(string query, int limit = 3)
        {
            var results = new Dictionary<string, object>();

            // Search movies
            var movies = await _movieRepository.SearchMoviesAsync(query, limit);
            results["movies"] = _mapper.Map<List<MovieDTO>>(movies);

            // Search theatres
            var theatres = await _theatreRepository.SearchTheatresAsync(query);
            results["theatres"] = _mapper.Map<List<TheatreDTO>>(theatres.Take(limit));

            // Search screenings
            var screenings = await _screeningRepository.SearchScreeningsAsync(query, limit);
            results["screenings"] = _mapper.Map<List<ScreeningDTO>>(screenings);

            // Add metadata
            results["query"] = query;
            results["totalResults"] = ((List<MovieDTO>)results["movies"]).Count +
                                     ((List<TheatreDTO>)results["theatres"]).Count +
                                     ((List<ScreeningDTO>)results["screenings"]).Count;

            return results;
        }

        public async Task<Dictionary<string, object>> SearchMoviesAsync(string query, int limit = 10)
        {
            var movies = await _movieRepository.SearchMoviesAsync(query, limit);

            var results = new Dictionary<string, object>
            {
                ["movies"] = _mapper.Map<List<MovieDTO>>(movies),
                ["query"] = query,
                ["totalResults"] = movies.Count()
            };

            return results;
        }

        public async Task<Dictionary<string, object>> SearchTheatresAsync(string query, int limit = 10)
        {
            var theatres = await _theatreRepository.SearchTheatresAsync(query);

            var results = new Dictionary<string, object>
            {
                ["theatres"] = _mapper.Map<List<TheatreDTO>>(theatres.Take(limit)),
                ["query"] = query,
                ["totalResults"] = theatres.Count()
            };

            return results;
        }

        public async Task<Dictionary<string, object>> SearchScreeningsAsync(string query, int limit = 10)
        {
            var screenings = await _screeningRepository.SearchScreeningsAsync(query, limit);

            var results = new Dictionary<string, object>
            {
                ["screenings"] = _mapper.Map<List<ScreeningDTO>>(screenings),
                ["query"] = query,
                ["totalResults"] = screenings.Count()
            };

            return results;
        }

        public async Task<Dictionary<string, object>> SearchUsersAsync(string query, int limit = 10)
        {
            var users = await _userRepository.FindByUsernameContainingIgnoreCaseOrEmailContainingIgnoreCaseOrFirstNameContainingIgnoreCaseOrLastNameContainingIgnoreCaseAsync(
                query, query, query, query);

            var results = new Dictionary<string, object>
            {
                ["users"] = _mapper.Map<List<UserDTO>>(users.Take(limit)),
                ["query"] = query,
                ["totalResults"] = users.Count()
            };

            return results;
        }

        public async Task<Dictionary<string, object>> GetSearchSuggestionsAsync(string query, int limit = 5)
        {
            var suggestions = new Dictionary<string, object>();

            // Movie title suggestions
            var movies = await _movieRepository.FindByTitleContainingIgnoreCaseAsync(query);
            var movieTitles = movies.Take(limit).Select(m => m.Title).ToList();

            // Theatre name suggestions
            var theatres = await _theatreRepository.FindByNameContainingIgnoreCaseAsync(query);
            var theatreNames = theatres.Take(limit).Select(t => t.Name).ToList();

            suggestions["movieTitles"] = movieTitles;
            suggestions["theatreNames"] = theatreNames;
            suggestions["query"] = query;

            return suggestions;
        }
    }

}
