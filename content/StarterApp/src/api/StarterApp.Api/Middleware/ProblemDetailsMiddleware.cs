using System.Net;
using Microsoft.AspNetCore.Http;
using StarterApp.Application.Common;

namespace StarterApp.API.Middleware;

/// <summary>
/// Problem Details middleware following RFC 7807 standard.
/// Provides structured error responses compatible with REST APIs.
/// </summary>
public class ProblemDetailsMiddleware(
    RequestDelegate next,
    ILogger<ProblemDetailsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            // Handle 404 responses
            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                await HandleNotFoundAsync(context);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in request to {Path}",
                LogSanitizer.Sanitize(context.Request.Path.Value));
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleNotFoundAsync(HttpContext context)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://httpstatuscodes.com/404",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = $"The requested resource '{context.Request.Path}' was not found",
            Instance = context.Request.Path,
            TraceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = "https://httpstatuscodes.com/500",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred. Please contact support with the trace ID.",
            Instance = context.Request.Path,
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "https://httpstatuscodes.com/400";
                problemDetails.Title = "Validation Failed";
                problemDetails.Detail = "One or more validation errors occurred";
                problemDetails.Extensions = new Dictionary<string, object>
                {
                    { "errors", validationEx.Errors }
                };
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Type = "https://httpstatuscodes.com/401";
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = "Authentication is required to access this resource";
                break;

            case KeyNotFoundException keyNotFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Type = "https://httpstatuscodes.com/404";
                problemDetails.Title = "Not Found";
                problemDetails.Detail = keyNotFoundEx.Message ?? "The requested resource was not found";
                break;

            case InvalidOperationException invalidOpEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "https://httpstatuscodes.com/400";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = invalidOpEx.Message ?? "The request is invalid";
                break;

            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "https://httpstatuscodes.com/400";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = argEx.Message ?? "One or more arguments are invalid";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // Don't expose internal error details
                break;
        }

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}

/// <summary>
/// Represents a Problem Details response following RFC 7807 standard.
/// </summary>
public class ProblemDetails
{
    /// <summary>A URI reference that identifies the problem type</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>A short, human-readable summary of the problem type</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The HTTP status code</summary>
    public int Status { get; set; }

    /// <summary>A human-readable explanation specific to this occurrence of the problem</summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>A URI reference that identifies the specific occurrence of the problem</summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>Unique identifier for tracing this specific error</summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>Additional properties related to the problem</summary>
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Extension member to register the problem details middleware.
/// </summary>
public static class ProblemDetailsMiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Adds problem details middleware to the application (RFC 7807 compliant).
        /// Should be registered early in the middleware pipeline.
        /// </summary>
        /// <returns>The application builder for chaining</returns>
        public IApplicationBuilder UseProblemDetails()
            => app.UseMiddleware<ProblemDetailsMiddleware>();
    }
}

