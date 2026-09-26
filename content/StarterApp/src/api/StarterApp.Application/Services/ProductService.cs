using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Sample application service showing the repository + unit-of-work pattern used across
/// this solution. Part of the Product sample slice — delete it with the rest.
/// </summary>
public class ProductService(
    IRepositoryBase<Product> repository,
    IUnitOfWork unitOfWork) : IProductService
{
    /// <inheritdoc />
    public async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products.OrderBy(p => p.Name).Select(ToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToResponse(product);
    }

    /// <inheritdoc />
    public async Task<ProductResponse?> CreateAsync(
        ProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await repository.AnyAsync(p => p.Sku == request.Sku, cancellationToken))
            return null;

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Price = request.Price,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        await repository.AddAsync(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    /// <inheritdoc />
    public async Task<ProductMutationResult> UpdateAsync(
        int id,
        ProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return ProductMutationResult.NotFound;

        // The SKU is unique, so reject a change that would collide with another row.
        if (!string.Equals(product.Sku, request.Sku, StringComparison.OrdinalIgnoreCase) &&
            await repository.AnyAsync(p => p.Sku == request.Sku, cancellationToken))
        {
            return ProductMutationResult.DuplicateSku;
        }

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Price = request.Price;
        product.IsActive = request.IsActive;

        await repository.UpdateAsync(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductMutationResult.Success;
    }

    /// <inheritdoc />
    public async Task<ProductMutationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return ProductMutationResult.NotFound;

        await repository.DeleteAsync(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductMutationResult.Success;
    }

    private static ProductResponse ToResponse(Product product) => new()
    {
        Id = product.Id,
        Sku = product.Sku,
        Name = product.Name,
        Price = product.Price,
        IsActive = product.IsActive,
        CreatedDate = product.CreatedDate
    };
}
