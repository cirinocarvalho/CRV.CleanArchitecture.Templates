using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// Sample application service contract showing how a use case is exposed to the API layer.
/// Part of the Product sample slice — delete it with the rest.
/// </summary>
public interface IProductService
{
    /// <summary>Returns every product, ordered by name.</summary>
    Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single product, or null when no product has that id.</summary>
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates a product. Returns null when the SKU is already taken.</summary>
    Task<ProductResponse?> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing product.</summary>
    Task<ProductMutationResult> UpdateAsync(int id, ProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a product.</summary>
    Task<ProductMutationResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
