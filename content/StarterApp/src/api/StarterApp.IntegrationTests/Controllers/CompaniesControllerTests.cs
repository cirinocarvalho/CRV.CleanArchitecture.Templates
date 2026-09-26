using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarterApp.Application.DTOs;
using StarterApp.IntegrationTests.Fixtures;
using Xunit;

namespace StarterApp.IntegrationTests.Controllers;

public class CompaniesControllerTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public CompaniesControllerTests(TestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_NoToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/companies");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_NonAdmin_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsAsync(_factory, Guid.NewGuid().ToString(), "User");
        var response = await client.GetAsync("/api/v1/companies");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_Admin_ReturnsOk()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsAsync(_factory, Guid.NewGuid().ToString(), "Admin");
        var response = await client.PostAsJsonAsync("/api/v1/companies", new CompanyRequest { Name = $"Co-{Guid.NewGuid():N}" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
