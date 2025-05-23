using System.Net;
using System.Text.Json;
using TheatreManagementSystem.DTOs;
using TheatreManagementSystem.Exceptions;

namespace TheatreManagementSystem.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>();

            switch (exception)
            {
                case ResourceNotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse<object>.ErrorResult(notFoundEx.Message);
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.ErrorResult("Unauthorized access");
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult(argEx.Message);
                    break;

                case InvalidOperationException invalidOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult(invalidOpEx.Message);
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse<object>.ErrorResult("Validation failed", validationEx.Errors);
                    break;

                case Microsoft.IdentityModel.Tokens.SecurityTokenException securityEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.ErrorResult("Token validation failed");
                    break;

                case TheatreManagementSystem.Exceptions.SecurityTokenException customSecurityEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = ApiResponse<object>.ErrorResult("Custom token validation failed");
                    break;

                case TimeoutException timeoutEx:
                    context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    response = ApiResponse<object>.ErrorResult("Request timeout");
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    if (_environment.IsDevelopment())
                    {
                        // In development, show detailed error information
                        response = ApiResponse<object>.ErrorResult($"Internal server error: {exception.Message}");

                        // Add stack trace in development
                        if (response.Errors == null)
                            response.Errors = new Dictionary<string, string>();
                        response.Errors["stackTrace"] = exception.StackTrace ?? "No stack trace available";
                        response.Errors["innerException"] = exception.InnerException?.Message ?? "No inner exception";
                    }
                    else
                    {
                        // In production, show generic error message
                        response = ApiResponse<object>.ErrorResult("An internal server error occurred");
                    }
                    break;
            }

            // Log the exception details
            _logger.LogError(exception,
                "Exception handled by middleware. Status: {StatusCode}, Message: {Message}",
                context.Response.StatusCode,
                exception.Message);

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}