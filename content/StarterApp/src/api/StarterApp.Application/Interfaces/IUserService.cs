using StarterApp.Application.Common;
using StarterApp.Application.DTOs;

namespace StarterApp.Application.Interfaces;

/// <summary>
/// User service interface defining authentication and user management operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Authenticates a user with provided email and password credentials.
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>Login result indicating success, invalid credentials, or an inactive account</returns>
    Task<LoginResult> LoginAsync(LoginRequest request);

    /// <summary>
    /// Registers a new user account with provided information.
    /// </summary>
    /// <param name="request">Registration request containing user details</param>
    /// <returns>True if registration successful, false otherwise</returns>
    Task<bool> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Changes the password for an authenticated user.
    /// </summary>
    /// <param name="request">Change password request with current and new password</param>
    /// <returns>Success flag with any validation messages to show the user</returns>
    Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request);

    /// <summary>
    /// Initiates password reset flow for user who forgot password.
    /// </summary>
    /// <param name="request">Forgot password request containing user email</param>
    /// <returns>Password reset token if user found, null otherwise</returns>
    Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>
    /// Completes password reset flow using valid reset token.
    /// </summary>
    /// <param name="request">Reset password request with token and new password</param>
    /// <returns>Success flag with any validation messages to show the user</returns>
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request);
}

