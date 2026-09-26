using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace StarterApp.API.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns appropriate HTTP responses without exposing sensitive information.
/// </summary>
public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred: {ExceptionMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions and returns appropriate error responses.
    /// Maps specific exception types to HTTP status codes and user-friendly messages.
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation error";
                response.Errors = validationEx.Errors;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Unauthorized access";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "The requested resource was not found";
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = invalidOpEx.Message ?? "Invalid operation";
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = argEx.Message ?? "Invalid argument";
                break;

            default:
                // Log full details of unexpected exceptions for debugging
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal error occurred. Please try again later.";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Represents an API error response returned to the client.
/// Does not contain sensitive information about the error.
/// </summary>
public class ErrorResponse
{
    /// <summary>HTTP status code</summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    /// <summary>User-friendly error message</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Unique identifier for tracking this specific error</summary>
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;

    /// <summary>When the error occurred (UTC)</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>Detailed validation errors (if applicable)</summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Custom exception for validation errors.
/// Allows passing structured validation error information.
/// </summary>
public class ValidationException(string message, Dictionary<string, string[]> errors) : Exception(message)
{
    public Dictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(Dictionary<string, string[]> errors)
        : this("Validation failed", errors)
    {
    }
}

/// <summary>
/// Extension member to register the global exception handler middleware.
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Adds global exception handling middleware to the application.
        /// Should be registered early in the middleware pipeline.
        /// </summary>
        /// <returns>The application builder for chaining</returns>
        public IApplicationBuilder UseGlobalExceptionHandler()
            => app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

