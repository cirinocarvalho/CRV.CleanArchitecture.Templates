using System.ComponentModel.DataAnnotations;

namespace StarterApp.Domain.Entities;

/// <summary>
/// Sample entity demonstrating the Domain → Application → Api layering.
/// Delete this file and the rest of the Product slice once you have your own entities.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Stock keeping unit — unique business identifier.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the product.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Whether the product is currently offered.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time the product was created (UTC).
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
