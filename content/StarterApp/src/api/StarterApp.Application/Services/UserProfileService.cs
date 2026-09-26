using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Links authenticated identity users with their assigned company.
/// </summary>
public class UserProfileService(
    IIdentityService identityService,
    IReadRepositoryBase<UserCompany> userCompanyRepo,
    IReadRepositoryBase<Company> companyRepo) : IUserProfileService
{
    /// <inheritdoc />
    public async Task<UserProfileResponse?> GetProfileByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var identityUser = await identityService.FindByEmailAsync(email);
        if (identityUser is null)
            return null;

        var company = string.Empty;
        var userId = Guid.Parse(identityUser.Id);
        var assignment = await userCompanyRepo.FirstOrDefaultAsync(uc => uc.UserId == userId, cancellationToken);
        if (assignment is not null)
        {
            var companyEntity = await companyRepo.GetByIdAsync(assignment.CompanyId, cancellationToken);
            company = companyEntity?.Name ?? string.Empty;
        }

        return new UserProfileResponse
        {
            IdentityUserId = identityUser.Id,
            Email = identityUser.Email,
            FullName = identityUser.FullName,
            Company = company,
            Roles = identityUser.Roles
        };
    }
}
