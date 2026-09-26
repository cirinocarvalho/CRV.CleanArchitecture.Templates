using System.ComponentModel.DataAnnotations;

namespace StarterApp.Domain.Entities;

/// <summary>
/// Assigns an identity user (AspNetUsers.Id) to a single company.
/// UserId is the primary key, enforcing one company per user.
/// </summary>
public class UserCompany
{
    [Key]
    public Guid UserId { get; set; }

    public int CompanyId { get; set; }

    public Company? Company { get; set; }
}
