using FluentAssertions;
using NSubstitute;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Services;
using Xunit;

namespace StarterApp.UnitTests.Services;

/// <summary>
/// Unit tests for RoleService.
/// Tests role management operations including creation and retrieval of roles.
/// </summary>
public class RoleServiceTests
{
    private readonly IRoleManagerService _roleManagerService = Substitute.For<IRoleManagerService>();
    private readonly RoleService _sut;

    /// <summary>
    /// Initializes a new instance of the RoleServiceTests class.
    /// Sets up mocked dependencies and creates the system under test.
    /// </summary>
    public RoleServiceTests()
        => _sut = new RoleService(_roleManagerService);

    [Fact]
    public async Task CreateRoleAsync_NewRole_ReturnsTrue()
    {
        _roleManagerService.CreateRoleAsync("Editor").Returns(true);

        var request = new RoleRequest { RoleName = "Editor" };

        var result = await _sut.CreateRoleAsync(request);

        result.Should().BeTrue();
        await _roleManagerService.Received(1).CreateRoleAsync("Editor");
    }

    [Fact]
    public async Task CreateRoleAsync_ExistingRole_ReturnsFalse()
    {
        _roleManagerService.CreateRoleAsync("Admin").Returns(false);

        var request = new RoleRequest { RoleName = "Admin" };

        var result = await _sut.CreateRoleAsync(request);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsRoleList()
    {
        List<string> roles = ["Admin", "Editor", "Viewer"];
        _roleManagerService.GetAllRolesAsync().Returns(roles);

        var result = await _sut.GetAllRolesAsync();

        result.Should().HaveCount(3);
        result.Should().ContainInOrder("Admin", "Editor", "Viewer");
    }
}