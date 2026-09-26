using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarterApp.Application.DTOs;
using StarterApp.IntegrationTests.Fixtures;
using Xunit;

namespace StarterApp.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for AuthController endpoints.
/// Tests authentication flows including registration, login, and password management.
/// </summary>
/// <param name="factory">Test web application factory for creating test client</param>
public class AuthControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// Tests that registration with valid request returns HTTP 200 OK.
    /// </summary>
    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var request = new RegisterRequest
        {
            Email = "integration@StarterApp.com",
            FullName = "Integration User",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that registration with duplicate email returns HTTP 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            Email = "duplicate@StarterApp.com",
            FullName = "Dup User",
            Password = "Password123"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that login with valid credentials returns JWT token.
    /// </summary>
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange � register first
        var registerRequest = new RegisterRequest
        {
            Email = "logintest@StarterApp.com",
            FullName = "Login User",
            Password = "Password123"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        // Registrations start inactive; activate before login.
        await factory.ActivateUserAsync("logintest@StarterApp.com");

        // Act
        var loginRequest = new LoginRequest
        {
            Email = "logintest@StarterApp.com",
            Password = "Password123"
        };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.Email.Should().Be("logintest@StarterApp.com");
    }

    /// <summary>
    /// Tests that login for a freshly registered (not yet activated) user returns HTTP 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task Login_InactiveUser_ReturnsForbidden()
    {
        var register = new RegisterRequest
        {
            Email = "inactive@StarterApp.com",
            FullName = "Inactive User",
            Password = "Password123"
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", register);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = "inactive@StarterApp.com",
            Password = "Password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Tests that login with wrong password returns HTTP 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "logintest@StarterApp.com",
            Password = "WrongPassword"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that forgot password endpoint always returns OK to prevent email enumeration.
    /// </summary>
    [Fact]
    public async Task ForgotPassword_AnyEmail_ReturnsOk()
    {
        var request = new ForgotPasswordRequest { Email = "anyone@StarterApp.com" };

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", request);

        // Always OK to prevent email enumeration
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}