using StarterApp.Application.Common;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Domain.Entities;

namespace StarterApp.Application.Services;

/// <summary>
/// Authentication service implementing user authentication operations.
/// Handles login, registration, and password management.
/// </summary>
public class AuthService(
    IIdentityService identityService,
    ITokenService tokenService,
    IReadRepositoryBase<Company> companyRepo,
    IRepositoryBase<UserCompany> userCompanyRepo,
    IUnitOfWork unitOfWork,
    IAccountEmailSender accountEmailSender) : IUserService
{
    private const string NoneCompanyName = "None";

    /// <summary>
    /// Authenticates a user with provided credentials.
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <returns>Login result: success, invalid credentials, or inactive account</returns>
    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await identityService.FindByEmailAsync(request.Email);
        if (user is null)
            return LoginResult.Failed;

        var valid = await identityService.CheckPasswordAsync(user, request.Password);
        if (!valid)
            return LoginResult.Failed;

        if (!user.IsActive)
            return LoginResult.NotActive;

        var token = await tokenService.CreateTokenAsync(user);

        return LoginResult.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Roles = user.Roles
        });
    }

    /// <summary>
    /// Registers a new user account as inactive and assigns the default "None" company.
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>True if registration successful, false otherwise</returns>
    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var created = await identityService.CreateUserAsync(request.Email, request.FullName, request.Password);
        if (!created)
            return false;

        await AssignDefaultCompanyAsync(request.Email);

        // Let the user know their account was created and is awaiting admin approval,
        // and alert the admins that there is a new account to review.
        await accountEmailSender.SendRegistrationReceivedAsync(request.Email, request.FullName);
        await accountEmailSender.SendNewRegistrationToAdminAsync(request.Email, request.FullName);
        return true;
    }

    private async Task AssignDefaultCompanyAsync(string email)
    {
        var user = await identityService.FindByEmailAsync(email);
        if (user is null)
            return;

        var none = await companyRepo.FirstOrDefaultAsync(c => c.Name == NoneCompanyName);
        if (none is null)
            return;

        var userId = Guid.Parse(user.Id);
        if (await userCompanyRepo.AnyAsync(uc => uc.UserId == userId))
            return;

        await userCompanyRepo.AddAsync(new UserCompany { UserId = userId, CompanyId = none.CompanyId });
        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Changes the password for an authenticated user.
    /// </summary>
    /// <param name="request">Change password request with current and new password</param>
    /// <returns>True if password change successful, false otherwise</returns>
    public async Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var result = await identityService.ChangePasswordAsync(
            request.Email,
            request.CurrentPassword,
            request.NewPassword);

        if (result.Success)
        {
            var user = await identityService.FindByEmailAsync(request.Email);
            await accountEmailSender.SendPasswordChangedAsync(request.Email, user?.FullName ?? string.Empty);
        }

        return result;
    }

    /// <summary>
    /// Initiates password reset flow for user who forgot password.
    /// </summary>
    /// <param name="request">Forgot password request with user email</param>
    /// <returns>Password reset token if user found, null otherwise</returns>
    public async Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var token = await identityService.GeneratePasswordResetTokenAsync(request.Email);

        // Email the reset link only when the account exists. The controller still
        // returns a generic response either way, to avoid email enumeration.
        if (token is not null)
        {
            var user = await identityService.FindByEmailAsync(request.Email);
            await accountEmailSender.SendPasswordResetAsync(request.Email, user?.FullName ?? string.Empty, token);
        }

        return token;
    }

    /// <summary>
    /// Completes password reset flow using valid reset token.
    /// </summary>
    /// <param name="request">Reset password request with token and new password</param>
    /// <returns>Success flag with any validation messages to show the user</returns>
    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        return await identityService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword);
    }
}
