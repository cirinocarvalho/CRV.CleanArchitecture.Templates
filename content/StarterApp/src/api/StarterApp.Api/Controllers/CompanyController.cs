using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Controller for company and user lookups.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CompanyController(ICompanyService service) : ControllerBase
{
    /// <summary>
    /// Gets distinct company names.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCompanyList(CancellationToken cancellationToken)
    {
        var companies = await service.GetCompanyListAsync(cancellationToken);
        return Ok(companies);
    }
}
