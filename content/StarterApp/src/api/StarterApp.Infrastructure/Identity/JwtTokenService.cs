using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StarterApp.Application.Interfaces;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// JWT token service for creating bearer tokens for authenticated users.
/// Implements JWT token generation with claims and expiration.
/// </summary>
/// <param name="settings">JWT configuration settings</param>
public class JwtTokenService(IOptions<JwtSettings> settings) : ITokenService
{
    private readonly JwtSettings _settings = settings.Value;

    /// <summary>
    /// Creates a JWT bearer token for the specified user.
    /// Token includes user claims and roles valid for the configured duration.
    /// </summary>
    /// <param name="user">The user to create token for</param>
    /// <returns>JWT bearer token string</returns>
    public Task<string> CreateTokenAsync(IIdentityUser user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        ];

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes),
            signingCredentials: credentials);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}