using System.Net;
using System.Text.Json;
using Vendix.Application.Common.Exceptions;

namespace Vendix.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions globally and converting them to appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Type = "NotFound",
                    Title = "Resource Not Found",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                }
            ),
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ValidationErrorResponse
                {
                    Type = "ValidationError",
                    Title = "One or more validation errors occurred",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = validationEx.Message,
                    Instance = context.Request.Path,
                    Errors = validationEx.Errors
                }
            ),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Type = "Conflict",
                    Title = "Resource Conflict",
                    Status = (int)HttpStatusCode.Conflict,
                    Detail = conflictEx.Message,
                    Instance = context.Request.Path
                }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalServerError",
                    Title = "An error occurred while processing your request",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                }
            )
        };

        // Log the exception with appropriate level
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "A handled exception occurred: {Message}", exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    /// <summary>
    /// Represents a standard error response following RFC 7807 Problem Details.
    /// </summary>
    private class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the error type URI.
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Gets or sets a short, human-readable summary of the problem.
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets a human-readable explanation specific to this occurrence.
        /// </summary>
        public string Detail { get; set; } = null!;

        /// <summary>
        /// Gets or sets the URI reference that identifies the specific occurrence.
        /// </summary>
        public string Instance { get; set; } = null!;
    }

    /// <summary>
    /// Represents a validation error response with detailed field errors.
    /// </summary>
    private class ValidationErrorResponse : ErrorResponse
    {
        /// <summary>
        /// Gets or sets the validation errors grouped by property name.
        /// </summary>
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}
