using System.ComponentModel.DataAnnotations;

namespace StarterApp.Domain.Entities;

/// <summary>
/// Master list of companies. Replaces free-text company names.
/// </summary>
public class Company
{
    [Key]
    public int CompanyId { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;
}
