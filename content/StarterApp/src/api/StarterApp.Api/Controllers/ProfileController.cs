using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// User profile controller for managing user profile information.
/// All endpoints require authentication.
/// </summary>
/// <param name="identityService">Identity service for profile operations</param>
/// <param name="userProfileService">User profile service for combined identity/company data</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProfileController(
    IIdentityService identityService,
    IUserProfileService userProfileService) : ControllerBase
{
    /// <summary>
    /// Returns the current user's profile from the JWT claims.
    /// Requires: Bearer token in Authorization header.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentProfile()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email is null)
            return Unauthorized();

        var user = await identityService.FindByEmailAsync(email);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.Roles
        });
    }

    /// <summary>
    /// Returns all claims embedded in the current JWT.
    /// Useful for debugging token contents.
    /// </summary>
    [HttpGet("claims")]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new
        {
            c.Type,
            c.Value
        });

        return Ok(claims);
    }

    /// <summary>
    /// Returns the current user's combined identity and company profile.
    /// Links the authenticated user to their company association.
    /// </summary>
    [HttpGet("details")]
    public async Task<IActionResult> GetProfileDetails(CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email is null)
            return Unauthorized();

        var profile = await userProfileService.GetProfileByEmailAsync(email, cancellationToken);
        if (profile is null)
            return NotFound(new { Message = "User not found." });

        return Ok(profile);
    }

    /// <summary>
    /// Admin-only endpoint. Looks up any user by email.
    /// Demonstrates role-based authorization reading from the JWT role claims.
    /// </summary>
    [HttpGet("user/{email}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await identityService.FindByEmailAsync(email);
        if (user is null)
            return NotFound(new { Message = "User not found." });

        var roles = await identityService.GetUserRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            Roles = roles
        });
    }
}