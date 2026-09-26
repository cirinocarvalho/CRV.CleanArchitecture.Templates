namespace StarterApp.Application.DTOs;

/// <summary>
/// Result of an external API call: success flag plus the returned value or error message.
/// </summary>
public record ApiResult
{
    public bool Status { get; init; }
    public string? Value { get; init; }
}
