namespace StarterApp.Application.DTOs;

/// <summary>
/// Sample request DTO for creating or updating a product.
/// Part of the Product sample slice — delete it with the rest.
/// </summary>
public record ProductRequest
{
    /// <summary>Stock keeping unit — unique business identifier.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Display name of the product.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Unit price.</summary>
    public decimal Price { get; init; }

    /// <summary>Whether the product is currently offered.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Sample response DTO for a product.
/// Part of the Product sample slice — delete it with the rest.
/// </summary>
public record ProductResponse
{
    /// <summary>Unique identifier for the product.</summary>
    public int Id { get; init; }

    /// <summary>Stock keeping unit — unique business identifier.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Display name of the product.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Unit price.</summary>
    public decimal Price { get; init; }

    /// <summary>Whether the product is currently offered.</summary>
    public bool IsActive { get; init; }

    /// <summary>Date and time the product was created (UTC).</summary>
    public DateTime CreatedDate { get; init; }
}

/// <summary>
/// Outcome of a product mutation, so the controller can map to the right status code
/// without the service layer knowing about HTTP.
/// </summary>
public enum ProductMutationResult
{
    /// <summary>The operation completed.</summary>
    Success,

    /// <summary>No product exists with the supplied id.</summary>
    NotFound,

    /// <summary>Another product already uses the supplied SKU.</summary>
    DuplicateSku
}
