using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using TheatreManagementSystem.DTOs;

namespace TheatreManagementSystem.Security
{
    public class JwtAuthEntryPoint : IAuthenticationHandler
    {
        private AuthenticationScheme? _scheme;
        private HttpContext? _context;

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _scheme = scheme;
            _context = context;
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            if (_context == null) return;

            _context.Response.ContentType = "application/json";
            _context.Response.StatusCode = 401;

            var response = ApiResponse.Error("Unauthorized - Authentication required");

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _context.Response.WriteAsync(json);
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            if (_context == null) return;

            _context.Response.ContentType = "application/json";
            _context.Response.StatusCode = 403;

            var response = ApiResponse.Error("Forbidden - Insufficient permissions");

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _context.Response.WriteAsync(json);
        }
    }
}