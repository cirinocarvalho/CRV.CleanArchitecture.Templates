using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Admin management of users: role toggling and company assignment.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserAdminService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetUsersAsync(cancellationToken));

    [HttpPost("{id:guid}/admin")]
    public async Task<IActionResult> GrantAdmin(Guid id, CancellationToken cancellationToken)
    {
        var ok = await service.GrantAdminAsync(id, cancellationToken);
        if (!ok)
            return NotFound(new { Message = "User not found." });
        return Ok(new { Message = "Admin role granted." });
    }

    [HttpDelete("{id:guid}/admin")]
    public async Task<IActionResult> RevokeAdmin(Guid id, CancellationToken cancellationToken)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (callerId is not null && Guid.TryParse(callerId, out var caller) && caller == id)
            return BadRequest(new { Message = "You cannot revoke your own Admin role." });

        var ok = await service.RevokeAdminAsync(id, cancellationToken);
        if (!ok)
            return NotFound(new { Message = "User not found." });
        return Ok(new { Message = "Admin role revoked." });
    }

    [HttpPut("{id:guid}/company")]
    public async Task<IActionResult> SetCompany(Guid id, [FromBody] SetCompanyRequest request, CancellationToken cancellationToken)
    {
        var ok = await service.SetCompanyAsync(id, request.CompanyId, cancellationToken);
        if (!ok)
            return BadRequest(new { Message = "Unknown company." });
        return Ok(new { Message = "Company assigned." });
    }

#if (useLocalIdentity)
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var ok = await service.ResetPasswordAsync(id, cancellationToken);
        if (!ok)
            return NotFound(new { Message = "User not found." });
        return Ok(new { Message = "Password reset email sent." });
    }

#endif
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var ok = await service.SetActiveAsync(id, true, cancellationToken);
        if (!ok)
            return NotFound(new { Message = "User not found." });
        return Ok(new { Message = "User activated." });
    }

    [HttpDelete("{id:guid}/activate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (callerId is not null && Guid.TryParse(callerId, out var caller) && caller == id)
            return BadRequest(new { Message = "You cannot deactivate your own account." });

        var ok = await service.SetActiveAsync(id, false, cancellationToken);
        if (!ok)
            return NotFound(new { Message = "User not found." });
        return Ok(new { Message = "User deactivated." });
    }
}
