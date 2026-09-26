using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Admin CRUD over the company master table.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
public class CompaniesController(ICompanyAdminService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CompanyRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request.Name, cancellationToken);
        if (created is null)
            return BadRequest(new { Message = "A company with that name already exists." });
        return Ok(created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Rename(int id, [FromBody] CompanyRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RenameAsync(id, request.Name, cancellationToken);
        return result switch
        {
            CompanyMutationResult.Success => Ok(new { Message = "Company updated." }),
            CompanyMutationResult.NotFound => NotFound(new { Message = "Company not found." }),
            _ => BadRequest(new { Message = "A company with that name already exists." })
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result switch
        {
            CompanyMutationResult.Success => Ok(new { Message = "Company deleted." }),
            CompanyMutationResult.NotFound => NotFound(new { Message = "Company not found." }),
            _ => Conflict(new { Message = "Company is assigned to one or more users." })
        };
    }
}
