using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StarterApp.API.Middleware;
using Xunit;

namespace StarterApp.IntegrationTests.Security;

/// <summary>
/// Integration tests to verify security headers are correctly added to HTTP responses.
/// </summary>
public class SecurityHeadersTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetRequest_ShouldIncludeXFrameOptionsHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludeXContentTypeOptionsHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludeXXSSProtectionHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.GetValues("X-XSS-Protection").Should().Contain("1; mode=block");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludeStrictTransportSecurityHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("Strict-Transport-Security");
        var hstsValue = response.Headers.GetValues("Strict-Transport-Security").FirstOrDefault();
        hstsValue.Should().Contain("max-age=31536000");
        hstsValue.Should().Contain("includeSubDomains");
        hstsValue.Should().Contain("preload");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludeContentSecurityPolicyHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("Content-Security-Policy");
        var cspValue = response.Headers.GetValues("Content-Security-Policy").FirstOrDefault();
        cspValue.Should().Contain("default-src 'self'");
        cspValue.Should().Contain("script-src 'self'");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludeReferrerPolicyHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludePermissionsPolicyHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("Permissions-Policy");
        var ppValue = response.Headers.GetValues("Permissions-Policy").FirstOrDefault();
        ppValue.Should().Contain("geolocation=()");
        ppValue.Should().Contain("microphone=()");
        ppValue.Should().Contain("camera=()");
    }

    [Fact]
    public async Task GetRequest_ShouldNotIncludeServerHeader()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().NotContainKey("Server");
    }

    [Fact]
    public async Task GetRequest_ShouldIncludeCacheControlHeaders()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/v1/profile");

        // Assert
        response.Headers.Should().ContainKey("Cache-Control");
        response.Headers.GetValues("Cache-Control").Should().Contain(header => header.Contains("no-store"));
    }

    [Fact]
    public async Task PostRequest_ShouldIncludeAllSecurityHeaders()
    {
        // Arrange
        var request = new
        {
            email = "test@example.com",
            password = "TestPassword123!"
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/auth/login", content);

        // Assert - Verify all critical headers are present
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.Should().ContainKey("Strict-Transport-Security");
        response.Headers.Should().ContainKey("Content-Security-Policy");
        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.Should().ContainKey("Permissions-Policy");
    }

    [Theory]
    [InlineData("/api/v1/auth/login")]
    [InlineData("/api/v1/profile")]
    [InlineData("/api/v1/profile/me")]
    public async Task AllEndpoints_ShouldIncludeSecurityHeaders(string endpoint)
    {
        // Arrange & Act
        var response = await _client.GetAsync(endpoint);

        // Assert - At minimum, headers should be present
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("Strict-Transport-Security");
    }
}

/// <summary>
/// Unit tests for SecurityHeadersMiddleware.
/// </summary>
public class SecurityHeadersMiddlewareUnitTests
{
    [Fact]
    public async Task Middleware_ShouldAddAllRequiredSecurityHeaders()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var called = false;

        RequestDelegate next = (ctx) =>
        {
            called = true;
            return Task.CompletedTask;
        };

        var logger = new MockLogger<SecurityHeadersMiddleware>();
        var middleware = new SecurityHeadersMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        called.Should().BeTrue();
        httpContext.Response.Headers.Should().ContainKey("X-Frame-Options");
        httpContext.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        httpContext.Response.Headers.Should().ContainKey("X-XSS-Protection");
        httpContext.Response.Headers.Should().ContainKey("Strict-Transport-Security");
        httpContext.Response.Headers.Should().ContainKey("Content-Security-Policy");
        httpContext.Response.Headers.Should().ContainKey("Referrer-Policy");
        httpContext.Response.Headers.Should().ContainKey("Permissions-Policy");
        httpContext.Response.Headers.Should().ContainKey("Cache-Control");
    }

    [Fact]
    public async Task Middleware_ShouldRemoveServerHeader()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Headers["Server"] = "TestServer/1.0";

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var logger = new MockLogger<SecurityHeadersMiddleware>();
        var middleware = new SecurityHeadersMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.Headers.Should().NotContainKey("Server");
    }

    [Fact]
    public async Task Middleware_ShouldRemovePoweredByHeader()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Headers["X-Powered-By"] = "ASP.NET";

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var logger = new MockLogger<SecurityHeadersMiddleware>();
        var middleware = new SecurityHeadersMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        httpContext.Response.Headers.Should().NotContainKey("X-Powered-By");
    }

    [Fact]
    public async Task XFrameOptionsHeader_ShouldBeDeny()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var logger = new MockLogger<SecurityHeadersMiddleware>();
        var middleware = new SecurityHeadersMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var headerValue = httpContext.Response.Headers["X-Frame-Options"].ToString();
        headerValue.Should().Be("DENY");
    }

    [Fact]
    public async Task StrictTransportSecurityHeader_ShouldIncludeOneYear()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        RequestDelegate next = (ctx) => Task.CompletedTask;
        var logger = new MockLogger<SecurityHeadersMiddleware>();
        var middleware = new SecurityHeadersMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var headerValue = httpContext.Response.Headers["Strict-Transport-Security"].ToString();
        headerValue.Should().Contain("max-age=31536000"); // 1 year in seconds
    }
}

/// <summary>
/// Mock logger for unit testing.
/// </summary>
public class MockLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

