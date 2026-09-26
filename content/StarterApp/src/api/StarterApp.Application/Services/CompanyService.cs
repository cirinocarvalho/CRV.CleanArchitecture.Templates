using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Application service for company lookups.
/// </summary>
public class CompanyService(IReadRepositoryBase<Company> repository) : ICompanyService
{
    public async Task<List<string>> GetCompanyListAsync(CancellationToken cancellationToken = default)
    {
        var companies = await repository.GetAllAsync(cancellationToken);
        return companies.Select(c => c.Name).OrderBy(n => n).ToList();
    }
}
