using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

public enum CompanyMutationResult { Success, NotFound, DuplicateName, InUse }

public interface ICompanyAdminService
{
    Task<List<CompanyResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CompanyResponse?> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task<CompanyMutationResult> RenameAsync(int companyId, string name, CancellationToken cancellationToken = default);
    Task<CompanyMutationResult> DeleteAsync(int companyId, CancellationToken cancellationToken = default);
}
