using System.Security.Claims;
using TheatreManagementSystem.Security;

namespace TheatreManagementSystem.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, JwtTokenProvider tokenProvider, UserDetailsService userDetailsService)
        {
            var token = GetJwtFromRequest(context.Request);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    if (tokenProvider.ValidateToken(token))
                    {
                        // Get username from token
                        var username = tokenProvider.GetUsernameFromToken(token);

                        if (!string.IsNullOrEmpty(username))
                        {
                            // Create claims principal
                            var principal = await userDetailsService.CreatePrincipalAsync(username);

                            if (principal != null)
                            {
                                // Set authentication in context
                                context.User = principal;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not set user authentication in security context");
                }
            }

            await _next(context);
        }

        private string? GetJwtFromRequest(HttpRequest request)
        {
            var bearerToken = request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(bearerToken) && bearerToken.StartsWith("Bearer "))
            {
                return bearerToken.Substring(7);
            }

            return null;
        }
    }

    // Extension method to register the middleware
    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }
}