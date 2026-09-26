namespace StarterApp.Application.DTOs;

public record CompanyRequest
{
    public string Name { get; init; } = string.Empty;
}
