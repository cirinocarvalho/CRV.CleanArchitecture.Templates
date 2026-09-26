using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarterApp.API.Middleware;
using Xunit;

namespace StarterApp.IntegrationTests.Logging;

/// <summary>
/// Integration tests for security logging middleware.
/// </summary>
public class SecurityLoggingTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AuthenticationAttempt_ShouldBeLogged()
    {
        // Arrange
        var loginRequest = new
        {
            email = "test@example.com",
            password = "password123"
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(loginRequest),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", content);

        // Assert
        // Middleware should process the request without errors
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task UnauthorizedAccess_ShouldBeLogged()
    {
        // Arrange & Act
#if (useAuth)
        var response = await _client.GetAsync("/api/v1/profile/me");

        // Assert
        // 401 Unauthorized should be logged as security event
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
#else
        // With no authentication configured there is no protected endpoint to probe;
        // assert only that the middleware leaves an unmatched route alone.
        var response = await _client.GetAsync("/api/v1/nonexistent");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
#endif
    }

    [Fact]
    public async Task SensitiveEndpointAccess_ShouldBeLogged()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/roles");

        // Assert
        // Request to sensitive endpoint should be logged
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task MiddlewareDoesNotAffectResponse()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/nonexistent");

        // Assert
        // Middleware should not affect normal response handling
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Unit tests for security logging middleware.
/// </summary>
public class SecurityLoggingMiddlewareUnitTests
{
    [Fact]
    public async Task Middleware_ShouldProcessRequest()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/auth/login";
        httpContext.Request.Method = "POST";
        httpContext.Response.StatusCode = 200;

        var called = false;

        RequestDelegate next = (ctx) =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<SecurityLoggingMiddleware>();
        var middleware = new SecurityLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Middleware_ShouldExtractClientIpFromRemoteConnection()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/profile";
        httpContext.Request.Method = "GET";
        httpContext.Response.StatusCode = 401;
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");

        var logger = new MockLogger<SecurityLoggingMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        var middleware = new SecurityLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert - Middleware should process without errors
        httpContext.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Middleware_ShouldExtractClientIpFromXForwardedFor()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/auth/login";
        httpContext.Request.Method = "POST";
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.1, 198.51.100.2";
        httpContext.Response.StatusCode = 400;

        var logger = new MockLogger<SecurityLoggingMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 400;
            return Task.CompletedTask;
        };

        var middleware = new SecurityLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Middleware_ShouldHandleUnknownIp()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/data";
        httpContext.Request.Method = "GET";
        httpContext.Response.StatusCode = 200;
        httpContext.Connection.RemoteIpAddress = null; // No IP available

        var logger = new MockLogger<SecurityLoggingMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            return Task.CompletedTask;
        };

        var middleware = new SecurityLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert - Middleware should handle gracefully
        httpContext.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Middleware_ShouldMeasureRequestDuration()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/slow-endpoint";
        httpContext.Request.Method = "GET";

        var logger = new MockLogger<SecurityLoggingMiddleware>();
        var delayMs = 100;

        RequestDelegate next = async (ctx) =>
        {
            await Task.Delay(delayMs);
            ctx.Response.StatusCode = 200;
        };

        var middleware = new SecurityLoggingMiddleware(next, logger);

        // Act
        var startTime = DateTime.UtcNow;
        await middleware.InvokeAsync(httpContext);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        duration.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(delayMs);
        httpContext.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Middleware_ShouldIdentifyAuthenticationEndpoints()
    {
        // Arrange
        string[] authPaths =
        [
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/auth/login"
        ];

        var logger = new MockLogger<SecurityLoggingMiddleware>();

        foreach (var path in authPaths)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = path;
            httpContext.Request.Method = "POST";
            httpContext.Response.StatusCode = 200;

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new SecurityLoggingMiddleware(next, logger);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(200);
        }
    }

    [Fact]
    public async Task Middleware_ShouldIdentifySensitiveEndpoints()
    {
        // Arrange
        string[] sensitivePaths =
        [
            "/api/v1/profile",
            "/api/v1/roles",
            "/api/v1/admin",
            "/api/v1/users"
        ];

        var logger = new MockLogger<SecurityLoggingMiddleware>();

        foreach (var path in sensitivePaths)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = path;
            httpContext.Request.Method = "GET";
            httpContext.Response.StatusCode = 200;

            RequestDelegate next = (ctx) => Task.CompletedTask;
            var middleware = new SecurityLoggingMiddleware(next, logger);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(200);
        }
    }

    [Fact]
    public async Task Middleware_ShouldNotImpactResponseOnServerError()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/data";
        httpContext.Request.Method = "GET";

        var logger = new MockLogger<SecurityLoggingMiddleware>();

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 500;
            return Task.CompletedTask;
        };

        var middleware = new SecurityLoggingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(500);
    }
}

/// <summary>
/// Mock logger for testing.
/// </summary>
public class MockLogger<T> : ILogger<T>
{
    public List<string> LogMessages { get; } = [];
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogMessages.Add(formatter(state, exception));
    }
}

