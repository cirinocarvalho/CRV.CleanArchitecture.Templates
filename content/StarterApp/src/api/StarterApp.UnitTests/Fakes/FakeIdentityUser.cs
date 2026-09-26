using StarterApp.Application.Interfaces;

namespace StarterApp.UnitTests.Fakes;

/// <summary>
/// Fake implementation of IIdentityUser for unit testing.
/// Provides a simple in-memory representation of a user without database dependencies.
/// </summary>
public class FakeIdentityUser : IIdentityUser
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of roles assigned to the user.
    /// </summary>
    public IList<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}