using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Exceptions;

namespace TheatreManagementSystem.Exceptions
{
    /// <summary>
    /// Global exception handler for controllers
    /// Matches Spring Boot's @ControllerAdvice functionality
    /// </summary>
    public class GlobalExceptionHandler : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An exception occurred in controller: {ControllerName}.{ActionName}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"]);

            var response = HandleException(context.Exception);
            context.Result = response.Result;
            context.ExceptionHandled = true;
        }

        private (IActionResult Result, HttpStatusCode StatusCode) HandleException(Exception exception)
        {
            return exception switch
            {
                ResourceNotFoundException ex => (
                    new NotFoundObjectResult(ApiResponse.Error(ex.Message)),
                    HttpStatusCode.NotFound
                ),

                ValidationException ex => (
                    new BadRequestObjectResult(ApiResponse.Error(ex.Message, ex.Errors)),
                    HttpStatusCode.BadRequest
                ),

                BusinessLogicException ex => (
                    new BadRequestObjectResult(ApiResponse.Error(ex.Message)),
                    HttpStatusCode.BadRequest
                ),

                ConflictException ex => (
                    new ConflictObjectResult(ApiResponse.Error(ex.Message)),
                    HttpStatusCode.Conflict
                ),

                AuthenticationException ex => (
                    new UnauthorizedObjectResult(ApiResponse.Error("Authentication failed")),
                    HttpStatusCode.Unauthorized
                ),

                AuthorizationException ex => (
                    new ObjectResult(ApiResponse.Error("Access denied")) { StatusCode = 403 },
                    HttpStatusCode.Forbidden
                ),

                Microsoft.IdentityModel.Tokens.SecurityTokenException ex => (
                    new UnauthorizedObjectResult(ApiResponse.Error("Invalid or expired token")),
                    HttpStatusCode.Unauthorized
                ),

                ServiceUnavailableException ex => (
                    new ObjectResult(ApiResponse.Error($"Service temporarily unavailable: {ex.ServiceName}")) { StatusCode = 503 },
                    HttpStatusCode.ServiceUnavailable
                ),

                TimeoutException ex => (
                    new ObjectResult(ApiResponse.Error("Request timeout")) { StatusCode = 408 },
                    HttpStatusCode.RequestTimeout
                ),

                ArgumentException ex => (
                    new BadRequestObjectResult(ApiResponse.Error(ex.Message)),
                    HttpStatusCode.BadRequest
                ),

                InvalidOperationException ex => (
                    new BadRequestObjectResult(ApiResponse.Error(ex.Message)),
                    HttpStatusCode.BadRequest
                ),

                UnauthorizedAccessException ex => (
                    new UnauthorizedObjectResult(ApiResponse.Error("Unauthorized access")),
                    HttpStatusCode.Unauthorized
                ),

                _ => HandleGenericException(exception)
            };
        }

        private (IActionResult Result, HttpStatusCode StatusCode) HandleGenericException(Exception exception)
        {
            if (_environment.IsDevelopment())
            {
                // In development, provide detailed error information
                var errors = new Dictionary<string, string>
                {
                    ["exception"] = exception.GetType().Name,
                    ["stackTrace"] = exception.StackTrace ?? "No stack trace available"
                };

                if (exception.InnerException != null)
                {
                    errors["innerException"] = exception.InnerException.Message;
                }

                return (
                    new ObjectResult(ApiResponse.Error($"Internal server error: {exception.Message}", errors)) { StatusCode = 500 },
                    HttpStatusCode.InternalServerError
                );
            }
            else
            {
                // In production, return generic error message
                return (
                    new ObjectResult(ApiResponse.Error("An internal server error occurred")) { StatusCode = 500 },
                    HttpStatusCode.InternalServerError
                );
            }
        }
    }

    /// <summary>
    /// Extension method to register the global exception handler
    /// </summary>
    public static class GlobalExceptionHandlerExtensions
    {
        public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
        {
            services.AddScoped<GlobalExceptionHandler>();
            return services;
        }
    }
}