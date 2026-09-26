namespace StarterApp.Application.DTOs;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    Inactive
}

/// <summary>
/// Outcome of a login attempt, distinguishing bad credentials from an inactive account.
/// </summary>
public record LoginResult
{
    public LoginStatus Status { get; init; }
    public AuthResponse? Response { get; init; }

    public static LoginResult Ok(AuthResponse response) =>
        new() { Status = LoginStatus.Success, Response = response };

    public static LoginResult Failed { get; } =
        new() { Status = LoginStatus.InvalidCredentials };

    public static LoginResult NotActive { get; } =
        new() { Status = LoginStatus.Inactive };
}
