using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

public interface IUserAdminService
{
    Task<List<UserListItem>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> GrantAdminAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeAdminAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> SetCompanyAsync(Guid userId, int companyId, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default);
#if (useLocalIdentity)
    Task<bool> ResetPasswordAsync(Guid userId, CancellationToken cancellationToken = default);
#endif
}
