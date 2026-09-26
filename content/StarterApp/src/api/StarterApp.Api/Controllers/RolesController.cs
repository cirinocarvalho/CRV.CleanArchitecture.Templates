using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Roles controller for managing user roles and permissions.
/// All endpoints require authentication and Admin role authorization.
/// </summary>
/// <param name="roleService">Role service for role management operations</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    /// <summary>
    /// Creates a new role.
    /// Requires: Admin role.
    /// </summary>
    /// <param name="request">Role creation request with role details</param>
    /// <returns>
    /// 200 OK if role created successfully.
    /// 400 Bad Request if role already exists or creation fails.
    /// 401 Unauthorized if not authenticated.
    /// 403 Forbidden if user doesn't have Admin role.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleRequest request)
    {
        var result = await roleService.CreateRoleAsync(request);
        if (!result)
            return BadRequest(new { Message = "Role already exists or creation failed." });

        return Ok(new { Message = "Role created successfully." });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await roleService.GetAllRolesAsync();
        return Ok(roles);
    }
}