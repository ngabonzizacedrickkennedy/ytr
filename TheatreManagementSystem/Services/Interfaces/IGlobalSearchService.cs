namespace TheatreManagementSystem.Services.Interfaces
{
    public interface IGlobalSearchService
    {
        Task<Dictionary<string, object>> GlobalSearchAsync(string query, int limit = 3);
        Task<Dictionary<string, object>> SearchMoviesAsync(string query, int limit = 10);
        Task<Dictionary<string, object>> SearchTheatresAsync(string query, int limit = 10);
        Task<Dictionary<string, object>> SearchScreeningsAsync(string query, int limit = 10);
        Task<Dictionary<string, object>> SearchUsersAsync(string query, int limit = 10);
        Task<Dictionary<string, object>> GetSearchSuggestionsAsync(string query, int limit = 5);
    }
}
