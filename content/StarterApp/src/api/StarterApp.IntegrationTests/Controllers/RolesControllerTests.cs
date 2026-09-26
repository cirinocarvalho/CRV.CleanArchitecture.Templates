using System.Net;
using FluentAssertions;
using StarterApp.IntegrationTests.Fixtures;
using Xunit;

namespace StarterApp.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for RolesController endpoints.
/// Tests role management operations including authorization checks.
/// </summary>
/// <param name="factory">Test web application factory for creating test client</param>
public class RolesControllerTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// Tests that accessing roles endpoint without authentication token returns HTTP 401 Unauthorized.
    /// </summary>
    [Fact]
    public async Task GetAllRoles_NoToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/roles");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}