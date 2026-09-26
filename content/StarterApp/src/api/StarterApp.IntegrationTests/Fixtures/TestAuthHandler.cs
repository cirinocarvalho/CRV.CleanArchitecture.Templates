using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StarterApp.IntegrationTests.Fixtures;

/// <summary>
/// Stands in for Entra during integration tests.
/// </summary>
/// <remarks>
/// The JWT flavour can sign its own tokens, so its tests mint a real one. Entra tokens are
/// signed by Microsoft and cannot be produced offline, so the whole authentication scheme is
/// replaced instead: the handler reads the caller's identity from request headers and issues
/// exactly the claims <c>EntraClaimsTransformation</c> would have produced.
/// </remarks>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestScheme";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RolesHeader = "X-Test-Roles";
    public const string EmailHeader = "X-Test-Email";
    public const string NameHeader = "X-Test-Name";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // No header means an anonymous caller, so endpoints without [Authorize] stay
        // reachable and protected ones still return 401.
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId))
            return Task.FromResult(AuthenticateResult.NoResult());

        // Defaults match the identity the JWT flavour's helper mints, so tests that only
        // care about "some authenticated admin" read the same either way. Tests that look
        // the caller up in the database override them.
        var email = Request.Headers.TryGetValue(EmailHeader, out var emailHeader)
            ? emailHeader.ToString()
            : "admin@example.com";

        var name = Request.Headers.TryGetValue(NameHeader, out var nameHeader)
            ? nameHeader.ToString()
            : "Admin User";

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name)
        ];

        if (Request.Headers.TryGetValue(RolesHeader, out var roles))
        {
            claims.AddRange(roles.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(role => new Claim(ClaimTypes.Role, role)));
        }

        var identity = new ClaimsIdentity(claims, SchemeName, ClaimTypes.Name, ClaimTypes.Role);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
