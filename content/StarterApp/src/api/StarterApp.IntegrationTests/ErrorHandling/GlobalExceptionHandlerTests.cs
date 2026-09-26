using FluentAssertions;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarterApp.API.Middleware;
using Xunit;

namespace StarterApp.IntegrationTests.ErrorHandling;

/// <summary>
/// Integration tests for global exception handling.
/// </summary>
public class GlobalExceptionHandlerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task UnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange & Act - Hit test endpoint that throws UnauthorizedAccessException
        var response = await _client.GetAsync("/test-errors/unauthorized");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        error.Should().NotBeNull();
        error!.StatusCode.Should().Be(401);
        error.Message.Should().NotBeNullOrEmpty();
        error.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvalidRequest_ShouldReturn400()
    {
        // Arrange & Act - Hit test endpoint that throws InvalidOperationException
        var response = await _client.GetAsync("/test-errors/bad-request");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        error.Should().NotBeNull();
        error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ErrorResponse_ShouldNotContainSensitiveStackTrace()
    {
        // Arrange & Act - Hit test endpoint that throws a generic Exception
        var response = await _client.GetAsync("/test-errors/internal-error");
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert - Ensure no stack trace or sensitive info in response
        responseContent.Should().NotContain("StackTrace");
        responseContent.Should().NotContain("at ");
        responseContent.Should().NotContain("Exception");
        responseContent.Should().NotContain("System.");
    }

    [Fact]
    public async Task ErrorResponse_ShouldIncludeTraceId()
    {
        // Arrange & Act - Hit test endpoint that throws InvalidOperationException
        var response = await _client.GetAsync("/test-errors/bad-request");
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        error.Should().NotBeNull();
        error!.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ErrorResponse_ContentType_ShouldBeJson()
    {
        // Arrange & Act - Hit test endpoint that throws InvalidOperationException
        var response = await _client.GetAsync("/test-errors/bad-request");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task NotFoundRequest_ShouldReturn404()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/nonexistent-endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ErrorResponse_ShouldIncludeTimestamp()
    {
        // Arrange & Act - Hit test endpoint that throws InvalidOperationException
        var beforeRequest = DateTime.UtcNow;
        var response = await _client.GetAsync("/test-errors/bad-request");
        var afterRequest = DateTime.UtcNow;

        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Assert
        error.Should().NotBeNull();
        error!.Timestamp.Should().BeOnOrAfter(beforeRequest);
        error.Timestamp.Should().BeOnOrBefore(afterRequest);
    }

    [Fact]
    public async Task ValidationErrorResponse_ShouldIncludeErrorDetails()
    {
        // Arrange & Act - Hit test endpoint that throws ValidationException
        var response = await _client.GetAsync("/test-errors/validation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        error.Should().NotBeNull();
        error!.Message.Should().NotBeNullOrEmpty();
        error.Errors.Should().NotBeNull();
        error.Errors.Should().ContainKey("Email");
        error.Errors.Should().ContainKey("Password");
    }
}

/// <summary>
/// Unit tests for global exception handler middleware.
/// </summary>
public class GlobalExceptionHandlerMiddlewareUnitTests
{
    [Fact]
    public async Task Middleware_ShouldCatchAndHandleExceptions()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();
        var called = false;

        RequestDelegate next = (ctx) =>
        {
            called = true;
            throw new InvalidOperationException("Test exception");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        called.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        httpContext.Response.ContentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task Middleware_ShouldReturnErrorResponseWithoutException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            throw new KeyNotFoundException("Item not found");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            throw new UnauthorizedAccessException("Access denied");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidationException_ShouldReturn400WithErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", ["Invalid email format"] },
            { "Password", ["Password is too short"] }
        };

        var httpContext = new DefaultHttpContext();
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            throw new ValidationException("Validation failed", errors);
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ErrorResponse_ShouldIncludeTraceId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.TraceIdentifier = "test-trace-id-12345";
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            throw new Exception("Test error");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("test-trace-id-12345");
    }

    [Fact]
    public async Task UnexpectedException_ShouldReturn500()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            throw new Exception("Unexpected error");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ErrorResponse_ShouldNotExposeStackTrace()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var logger = new MockLogger<GlobalExceptionHandlerMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            throw new Exception("Sensitive error details");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        responseBody.Should().NotContain("Sensitive error details");
        responseBody.Should().NotContain("StackTrace");
        responseBody.Should().NotContain("System.");
    }
}

/// <summary>
/// Unit tests for ErrorResponse class.
/// </summary>
public class ErrorResponseTests
{
    [Fact]
    public void ErrorResponse_ShouldBeSerializableToJson()
    {
        // Arrange
        var response = new ErrorResponse
        {
            StatusCode = 400,
            Message = "Test error",
            TraceId = "trace-123",
            Timestamp = DateTime.UtcNow,
            Errors = new Dictionary<string, string[]>
            {
                { "Field", ["Error message"] }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"statusCode\":400");
        json.Should().Contain("\"message\":\"Test error\"");
        json.Should().Contain("\"traceId\":\"trace-123\"");
    }

    [Fact]
    public void ErrorResponse_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var response = new ErrorResponse();

        // Assert
        response.StatusCode.Should().Be(0);
        response.Message.Should().Be(string.Empty);
        response.TraceId.Should().Be(string.Empty);
        response.Errors.Should().BeNull();
    }
}

/// <summary>
/// Mock logger for testing.
/// </summary>
public class MockLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

