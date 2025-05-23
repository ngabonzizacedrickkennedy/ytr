namespace TheatreManagementSystem.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public Dictionary<string, string>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, string? message = null, T? data = default, Dictionary<string, string>? errors = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors;
        }

        // Success factory methods
        public static ApiResponse<T> SuccessResult(T data)
        {
            return new ApiResponse<T>(true, null, data);
        }

        public static ApiResponse<T> SuccessResult(T data, string message)
        {
            return new ApiResponse<T>(true, message, data);
        }

        // Error factory methods
        public static ApiResponse<T> ErrorResult(string message)
        {
            return new ApiResponse<T>(false, message);
        }

        public static ApiResponse<T> ErrorResult(string message, Dictionary<string, string> errors)
        {
            return new ApiResponse<T>(false, message, default, errors);
        }
    }

    // Non-generic version for responses without data
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base()
        {
        }

        public ApiResponse(bool success, string? message = null, object? data = null, Dictionary<string, string>? errors = null)
            : base(success, message, data, errors)
        {
        }

        public static ApiResponse Success(string? message = null)
        {
            return new ApiResponse(true, message);
        }

        public static ApiResponse Error(string message)
        {
            return new ApiResponse(false, message);
        }

        public static ApiResponse Error(string message, Dictionary<string, string> errors)
        {
            return new ApiResponse(false, message, null, errors);
        }
    }
}