using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Admin operations over users: role toggling and company assignment.
/// </summary>
public class UserAdminService(
    IIdentityService identityService,
    IRepositoryBase<Company> companyRepo,
    IRepositoryBase<UserCompany> userCompanyRepo,
    IUnitOfWork unitOfWork,
    IAccountEmailSender accountEmailSender) : IUserAdminService
{
    private const string AdminRole = "Admin";

    public async Task<List<UserListItem>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await identityService.GetAllUsersAsync();
        var assignments = await userCompanyRepo.GetAllAsync(cancellationToken);
        var companies = await companyRepo.GetAllAsync(cancellationToken);

        var companyById = companies.ToDictionary(c => c.CompanyId, c => c.Name);
        var companyByUser = assignments.ToDictionary(a => a.UserId, a => a.CompanyId);

        return users.Select(u =>
        {
            var id = Guid.Parse(u.Id);
            int? companyId = companyByUser.TryGetValue(id, out var cid) ? cid : null;
            return new UserListItem
            {
                Id = id,
                Email = u.Email,
                FullName = u.FullName,
                Roles = u.Roles,
                CompanyId = companyId,
                CompanyName = companyId is int c && companyById.TryGetValue(c, out var name) ? name : null,
                IsActive = u.IsActive
            };
        }).ToList();
    }

    public async Task<bool> SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var ok = await identityService.SetActiveAsync(userId.ToString(), isActive);

        // Let the user know once an admin activates their account.
        if (ok && isActive)
        {
            var user = await identityService.FindByIdAsync(userId.ToString());
            if (user is not null)
                await accountEmailSender.SendAccountActivatedAsync(user.Email, user.FullName, cancellationToken);
        }

        return ok;
    }

#if (useLocalIdentity)
    public async Task<bool> ResetPasswordAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await identityService.FindByIdAsync(userId.ToString());
        if (user is null)
            return false;

        var token = await identityService.GeneratePasswordResetTokenAsync(user.Email);
        if (token is null)
            return false;

        await accountEmailSender.SendPasswordResetAsync(user.Email, user.FullName, token, cancellationToken);
        return true;
    }

#endif
    public Task<bool> GrantAdminAsync(Guid userId, CancellationToken cancellationToken = default)
        => identityService.AddToRoleByIdAsync(userId.ToString(), AdminRole);

    public Task<bool> RevokeAdminAsync(Guid userId, CancellationToken cancellationToken = default)
        => identityService.RemoveFromRoleByIdAsync(userId.ToString(), AdminRole);

    public async Task<bool> SetCompanyAsync(Guid userId, int companyId, CancellationToken cancellationToken = default)
    {
        if (!await companyRepo.AnyAsync(c => c.CompanyId == companyId, cancellationToken))
            return false;

        var existing = await userCompanyRepo.GetByIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            await userCompanyRepo.AddAsync(new UserCompany { UserId = userId, CompanyId = companyId });
        }
        else
        {
            existing.CompanyId = companyId;
            await userCompanyRepo.UpdateAsync(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
