namespace StarterApp.Application.DTOs;

public record CompanyResponse
{
    public int CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
}
