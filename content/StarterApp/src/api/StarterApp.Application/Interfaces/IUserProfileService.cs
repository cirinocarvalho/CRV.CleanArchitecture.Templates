using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// Resolves an authenticated identity user's combined profile, including their assigned company.
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Retrieves a combined profile containing identity data and the user's assigned company.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined profile if the identity user exists; null otherwise</returns>
    Task<UserProfileResponse?> GetProfileByEmailAsync(string email, CancellationToken cancellationToken = default);
}
