namespace StarterApp.Application.Interfaces;

/// <summary>
/// Service for company lookups.
/// </summary>
public interface ICompanyService
{
    Task<List<string>> GetCompanyListAsync(CancellationToken cancellationToken = default);
}
