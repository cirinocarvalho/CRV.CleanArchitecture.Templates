using System.Net;
using FluentAssertions;
using StarterApp.IntegrationTests.Fixtures;
using Xunit;

namespace StarterApp.IntegrationTests.Controllers;

public class UsersControllerTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public UsersControllerTests(TestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_NoToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_NonAdmin_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsAsync(_factory, Guid.NewGuid().ToString(), "User");
        var response = await client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_Admin_ReturnsOk()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsAsync(_factory, Guid.NewGuid().ToString(), "Admin");
        var response = await client.GetAsync("/api/v1/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RevokeOwnAdmin_ReturnsBadRequest()
    {
        var id = Guid.NewGuid().ToString();
        var client = _factory.CreateClient();
        await client.AuthenticateAsAsync(_factory, id, "Admin");
        var response = await client.DeleteAsync($"/api/v1/users/{id}/admin");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
