using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
#if (useEntra)
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StarterApp.Infrastructure.Identity;
#else
using StarterApp.Application.DTOs;
#endif
using StarterApp.IntegrationTests.Fixtures;
using Xunit;

namespace StarterApp.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for ProfileController endpoints.
/// Tests user profile operations including authentication, authorization, and claims management.
/// </summary>
/// <param name="factory">Test web application factory for creating test client</param>
public class ProfileControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// Tests that accessing profile endpoint without authentication token returns HTTP 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task GetCurrentProfile_NoToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/profile/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Tests that accessing profile endpoint with valid authentication token returns HTTP 200 OK with profile data.
    /// </summary>
    [Fact]
    public async Task GetCurrentProfile_WithToken_ReturnsProfile()
    {
        // Arrange � register + login
        var token = await RegisterAndLoginAsync("profile@StarterApp.com", "Profile User", "Password123");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/profile/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that accessing claims endpoint with valid token returns HTTP 200 OK with user claims.
    /// </summary>
    [Fact]
    public async Task GetClaims_WithToken_ReturnsClaims()
    {
        var token = await RegisterAndLoginAsync("claims@StarterApp.com", "Claims User", "Password123");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/profile/claims");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that non-admin users cannot access admin-only profile endpoints and receive HTTP 403 Forbidden.
    /// </summary>
    [Fact]
    public async Task GetUserByEmail_NonAdmin_ReturnsForbidden()
    {
        var token = await RegisterAndLoginAsync("nonadmin@StarterApp.com", "Regular User", "Password123");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/profile/user/someone@StarterApp.com");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Helper method to register a new user and login to obtain an authentication token.
    /// </summary>
    /// <param name="email">Email address for the new user</param>
    /// <param name="fullName">Full name for the new user</param>
    /// <param name="password">Password for the new user</param>
    /// <returns>JWT authentication token</returns>
#if (useEntra)
    private async Task<string> RegisterAndLoginAsync(string email, string fullName, string password)
    {
        // Entra has no registration or login endpoint to drive, so the account is seeded
        // straight into the store — the same thing EntraClaimsTransformation would do on a
        // first sign-in — and TestAuthHandler stands in for the token.
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true
            };

            await userManager.CreateAsync(user);
        }

        // The profile endpoints look the caller up by email claim, so it has to be the
        // seeded address rather than the handler's default.
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.EmailHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.NameHeader);

        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, user.Id.ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.NameHeader, fullName);

        // Returned only so the callers below read identically under both auth modes; the
        // test scheme ignores the Authorization header.
        return "test-token";
    }
#else
    private async Task<string> RegisterAndLoginAsync(string email, string fullName, string password)
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = email,
            FullName = fullName,
            Password = password
        });

        // Registrations start inactive; activate before login.
        await factory.ActivateUserAsync(email);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }
#endif
}