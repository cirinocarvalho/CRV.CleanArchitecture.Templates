using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Admin operations over the Company master table.
/// </summary>
public class CompanyAdminService(
    IRepositoryBase<Company> companyRepo,
    IRepositoryBase<UserCompany> userCompanyRepo,
    IUnitOfWork unitOfWork) : ICompanyAdminService
{
    public async Task<List<CompanyResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var companies = await companyRepo.GetAllAsync(cancellationToken);
        return companies
            .OrderBy(c => c.Name)
            .Select(c => new CompanyResponse { CompanyId = c.CompanyId, Name = c.Name })
            .ToList();
    }

    public async Task<CompanyResponse?> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        if (await companyRepo.AnyAsync(c => c.Name == name, cancellationToken))
            return null;

        var entity = await companyRepo.AddAsync(new Company { Name = name });
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CompanyResponse { CompanyId = entity.CompanyId, Name = entity.Name };
    }

    public async Task<CompanyMutationResult> RenameAsync(int companyId, string name, CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        var entity = await companyRepo.GetByIdAsync(companyId, cancellationToken);
        if (entity is null)
            return CompanyMutationResult.NotFound;

        if (await companyRepo.AnyAsync(c => c.Name == name && c.CompanyId != companyId, cancellationToken))
            return CompanyMutationResult.DuplicateName;

        entity.Name = name;
        await companyRepo.UpdateAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CompanyMutationResult.Success;
    }

    public async Task<CompanyMutationResult> DeleteAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var entity = await companyRepo.GetByIdAsync(companyId, cancellationToken);
        if (entity is null)
            return CompanyMutationResult.NotFound;

        if (await userCompanyRepo.AnyAsync(uc => uc.CompanyId == companyId, cancellationToken))
            return CompanyMutationResult.InUse;

        await companyRepo.DeleteAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CompanyMutationResult.Success;
    }
}
