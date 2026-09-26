using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace StarterApp.Infrastructure.Identity;

/// <summary>
/// Projects an authenticated Entra principal onto the local user record.
/// </summary>
/// <remarks>
/// Entra proves <em>who</em> the caller is; the database still decides <em>what they may
/// do</em>. This runs on every authenticated request and republishes exactly the claims the
/// JWT flavour issues — local id, email, name and roles — so controllers, services and
/// <c>[Authorize(Roles = ...)]</c> behave identically under either authentication mode.
///
/// The link between the two lives in <c>AspNetUserLogins</c>, keyed on the Entra object id
/// rather than on the email address, which a person can change.
/// </remarks>
public class EntraClaimsTransformation(UserManager<ApplicationUser> userManager) : IClaimsTransformation
{
    /// <summary>Provider name recorded against the external login.</summary>
    public const string LoginProvider = "Entra";

    /// <summary>
    /// Marks a principal as already projected. Claims transformation is not guaranteed to
    /// run once per request, and without this a second pass would duplicate every role.
    /// </summary>
    private const string TransformedClaim = "starterapp_local_identity";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        if (principal.HasClaim(c => c.Type == TransformedClaim))
            return principal;

        // 'oid' is the immutable per-tenant object id. 'sub' is only unique per
        // application, so it is a fallback rather than the first choice.
        var objectId = principal.FindFirstValue("oid")
            ?? principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(objectId))
            return principal;

        var user = await ResolveUserAsync(principal, objectId);

        // Deactivated accounts fall through with the raw Entra claims only: no local id and
        // no roles, so anything behind [Authorize(Roles = ...)] or a profile lookup fails
        // even though Entra itself was happy to authenticate them.
        if (user is null || !user.IsActive)
            return principal;

        var roles = await userManager.GetRolesAsync(user);

        // Rebuilt as a single identity rather than appended as a second one: ClaimsPrincipal
        // resolves FindFirst against its identities in order, so an appended identity would
        // lose to the token's own 'sub'/'name' claims and User.FindFirstValue would return
        // the Entra id where every caller expects the local one.
        var claims = principal.Claims
            .Where(c => c.Type is not (ClaimTypes.NameIdentifier or ClaimTypes.Email or ClaimTypes.Name or ClaimTypes.Role))
            .ToList();

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Email, user.Email ?? string.Empty));
        claims.Add(new Claim(ClaimTypes.Name, user.FullName));
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.Add(new Claim(TransformedClaim, "true"));

        var identity = new ClaimsIdentity(
            claims,
            principal.Identity.AuthenticationType,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Finds the local account for an Entra object id, linking or creating one on first
    /// sign-in. Provisioning here rather than through an admin screen is what lets a new
    /// starter reach the application at all — they arrive with no local record.
    /// </summary>
    private async Task<ApplicationUser?> ResolveUserAsync(ClaimsPrincipal principal, string objectId)
    {
        var user = await userManager.FindByLoginAsync(LoginProvider, objectId);
        if (user is not null)
            return user;

        var email = principal.FindFirstValue("preferred_username")
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email")
            ?? principal.FindFirstValue("upn");

        if (string.IsNullOrWhiteSpace(email))
            return null;

        var name = principal.FindFirstValue("name")
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? email;

        // An account may already exist from a previous password-based life, or have been
        // pre-created by an administrator so roles are waiting on first sign-in. Adopt it
        // instead of creating a duplicate that would trip the unique-email rule.
        user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                EmailConfirmed = true,
                IsActive = true
            };

            var created = await userManager.CreateAsync(user);
            if (!created.Succeeded)
            {
                // Two first requests can race. The loser re-reads the winner's row rather
                // than failing the request.
                user = await userManager.FindByEmailAsync(email);
                if (user is null)
                    return null;
            }
        }

        var linked = await userManager.AddLoginAsync(
            user, new UserLoginInfo(LoginProvider, objectId, LoginProvider));

        // A concurrent request may have linked it already, which is success as far as this
        // request is concerned.
        return linked.Succeeded || await userManager.FindByLoginAsync(LoginProvider, objectId) is not null
            ? user
            : null;
    }
}
