using StarterApp.Application.Interfaces;

namespace StarterApp.IntegrationTests.Fixtures;

/// <summary>Minimal IIdentityUser for minting test tokens.</summary>
public sealed class FakeIdentityUser : IIdentityUser
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Email { get; init; } = "test@example.com";
    public string FullName { get; init; } = "Test User";
    public IList<string> Roles { get; init; } = [];
    public bool IsActive { get; init; } = true;
}
