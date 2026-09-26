using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StarterApp.API.Extensions;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;

namespace StarterApp.API.Controllers;

/// <summary>
/// Authentication controller for user login, registration, and password management.
/// Handles all authentication-related HTTP requests.
/// </summary>
/// <param name="userService">User service for authentication operations</param>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user with provided credentials.
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>
    /// 200 OK with authentication response containing JWT token.
    /// 401 Unauthorized if credentials are invalid.
    /// </returns>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await userService.LoginAsync(request);
        return result.Status switch
        {
            LoginStatus.Success => Ok(result.Response),
            LoginStatus.Inactive => StatusCode(403,
                new { Message = "Your account is pending administrator approval." }),
            _ => Unauthorized(new { Message = "Invalid email or password." })
        };
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration request containing user details</param>
    /// <returns>
    /// 200 OK if registration successful.
    /// 400 Bad Request if registration fails (duplicate email, validation errors).
    /// </returns>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await userService.RegisterAsync(request);
        if (!result)
            return BadRequest(new { Message = "Registration failed." });

        return Ok(new { Message = "User registered successfully." });
    }

    /// <summary>
    /// Changes the password for the authenticated user.
    /// Requires authentication.
    /// </summary>
    /// <param name="request">Password change request with current and new password</param>
    /// <returns>
    /// 200 OK if password changed successfully.
    /// 400 Bad Request if current password is incorrect.
    /// 401 Unauthorized if user is not authenticated.
    /// </returns>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await userService.ChangePasswordAsync(request);
        if (!result.Success)
            return BadRequest(new
            {
                Message = MessageOrDefault(result.Messages, "Password change failed. Verify your current password.")
            });

        return Ok(new { Message = "Password changed successfully." });
    }

    /// <summary>
    /// Initiates password reset flow.
    /// Generates a password reset token for the user.
    /// </summary>
    /// <param name="request">Forgot password request containing user email</param>
    /// <returns>
    /// 200 OK regardless of whether the email exists (prevents email enumeration).
    /// When the account exists, the reset link is emailed to the user by the service.
    /// </returns>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthEmailPolicy)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        // The service generates the reset token and emails the link when the
        // account exists. We always return the same response to avoid revealing
        // whether an email is registered.
        await userService.ForgotPasswordAsync(request);

        return Ok(new { Message = "If the email exists, a reset link has been sent." });
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await userService.ResetPasswordAsync(request);
        if (!result.Success)
            return BadRequest(new
            {
                Message = MessageOrDefault(result.Messages, "Password reset failed. The link may be invalid or expired.")
            });

        return Ok(new { Message = "Password has been reset successfully." });
    }

    /// <summary>
    /// Joins service validation messages into a single string, falling back to a
    /// generic message when none are present.
    /// </summary>
    private static string MessageOrDefault(IEnumerable<string> messages, string fallback)
    {
        var text = string.Join(" ", messages);
        return string.IsNullOrWhiteSpace(text) ? fallback : text;
    }
}