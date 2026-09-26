namespace StarterApp.Application.Interfaces;

/// <summary>
/// Token service interface defining JWT token creation operations.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a JWT bearer token for authenticated user.
    /// Token includes user identity claims and roles.
    /// </summary>
    /// <param name="user">The identity user to create token for</param>
    /// <returns>JWT bearer token string</returns>
    Task<string> CreateTokenAsync(IIdentityUser user);
}


