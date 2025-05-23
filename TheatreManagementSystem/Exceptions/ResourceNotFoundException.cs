using System.Net;

namespace TheatreManagementSystem.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found
    /// Matches Spring Boot's ResourceNotFoundException
    /// </summary>
    public class ResourceNotFoundException : Exception
    {
        public ResourceNotFoundException(string message) : base(message)
        {
        }

        public ResourceNotFoundException(string resourceName, string fieldName, object fieldValue)
            : base($"{resourceName} not found with {fieldName}: '{fieldValue}'")
        {
        }

        public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string>();
        }

        public ValidationException(string message, Dictionary<string, string> errors) : base(message)
        {
            Errors = errors ?? new Dictionary<string, string>();
        }

        public ValidationException(Dictionary<string, string> errors) : base("Validation failed")
        {
            Errors = errors ?? new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Exception thrown when business logic rules are violated
    /// </summary>
    public class BusinessLogicException : Exception
    {
        public string ErrorCode { get; }

        public BusinessLogicException(string message) : base(message)
        {
            ErrorCode = "BUSINESS_LOGIC_ERROR";
        }

        public BusinessLogicException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessLogicException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "BUSINESS_LOGIC_ERROR";
        }
    }

    /// <summary>
    /// Exception thrown when authorization fails
    /// </summary>
    public class AuthorizationException : Exception
    {
        public AuthorizationException(string message) : base(message)
        {
        }

        public AuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when authentication fails
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message)
        {
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a resource conflict occurs (e.g., duplicate entries)
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }

        public ConflictException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an external service is unavailable
    /// </summary>
    public class ServiceUnavailableException : Exception
    {
        public string ServiceName { get; }

        public ServiceUnavailableException(string serviceName, string message) : base(message)
        {
            ServiceName = serviceName;
        }

        public ServiceUnavailableException(string serviceName, string message, Exception innerException)
            : base(message, innerException)
        {
            ServiceName = serviceName;
        }
    }
}