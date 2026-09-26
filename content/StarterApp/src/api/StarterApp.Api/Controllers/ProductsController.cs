using Asp.Versioning;
#if (useAuth)
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Mvc;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Sample CRUD controller showing the versioning, routing, and result-mapping conventions
/// used across this API. Part of the Product sample slice — delete it with the rest.
/// </summary>
/// <param name="service">Product application service.</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
#if (useAuth)
[Authorize]
#endif
public class ProductsController(IProductService service) : ControllerBase
{
    /// <summary>Returns every product.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetAllAsync(cancellationToken));

    /// <summary>Returns a single product by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await service.GetByIdAsync(id, cancellationToken);
        return product is null
            ? NotFound(new { Message = "Product not found." })
            : Ok(product);
    }

    /// <summary>Creates a product.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        if (created is null)
            return Conflict(new { Message = "A product with that SKU already exists." });

        return CreatedAtAction(nameof(GetById), new { id = created.Id, version = "1.0" }, created);
    }

    /// <summary>Updates an existing product.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result switch
        {
            ProductMutationResult.Success => Ok(new { Message = "Product updated." }),
            ProductMutationResult.NotFound => NotFound(new { Message = "Product not found." }),
            _ => Conflict(new { Message = "A product with that SKU already exists." })
        };
    }

    /// <summary>Deletes a product.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result switch
        {
            ProductMutationResult.Success => NoContent(),
            _ => NotFound(new { Message = "Product not found." })
        };
    }
}
