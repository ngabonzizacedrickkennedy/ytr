using Microsoft.AspNetCore.Authorization;
using TheatreManagementSystem.Configuration;
using TheatreManagementSystem.Middleware;
using TheatreManagementSystem.Security;

namespace TheatreManagementSystem.Configuration
{
    public static class SecurityConfig
    {
        public static void ConfigureSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            // Add JWT Authentication
            services.AddJwtAuthentication(configuration);

            // Add custom authorization policies
            services.AddCustomAuthorizationPolicies();

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowedOrigins", builder =>
                {
                    builder.WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5173",
                            "http://localhost:5174",
                            "http://localhost:3001"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
        }

        public static void UseSecurityMiddleware(this IApplicationBuilder app)
        {
            // Use CORS
            app.UseCors("AllowedOrigins");

            // Use Authentication
            app.UseAuthentication();

            // Use custom JWT middleware
            app.UseJwtMiddleware();

            // Use Authorization
            app.UseAuthorization();
        }
    }
}