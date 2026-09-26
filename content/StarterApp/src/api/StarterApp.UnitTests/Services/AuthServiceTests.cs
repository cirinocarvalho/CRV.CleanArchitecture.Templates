using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using StarterApp.Application.Common;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Application.Services;
using StarterApp.Domain.Entities;
using StarterApp.UnitTests.Fakes;
using Xunit;

namespace StarterApp.UnitTests.Services;

/// <summary>
/// Unit tests for AuthService.
/// Tests authentication operations including login, registration, and password management.
/// </summary>
public class AuthServiceTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IReadRepositoryBase<Company> _companyRepo = Substitute.For<IReadRepositoryBase<Company>>();
    private readonly IRepositoryBase<UserCompany> _userCompanyRepo = Substitute.For<IRepositoryBase<UserCompany>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IAccountEmailSender _accountEmailSender = Substitute.For<IAccountEmailSender>();
    private readonly AuthService _sut;

    public AuthServiceTests()
        => _sut = new AuthService(_identityService, _tokenService, _companyRepo, _userCompanyRepo, _uow, _accountEmailSender);

    [Fact]
    public async Task LoginAsync_ValidActiveCredentials_ReturnsSuccess()
    {
        var user = new FakeIdentityUser
        {
            Email = "test@StarterApp.com",
            FullName = "Test User",
            Roles = ["Admin"],
            IsActive = true
        };

        _identityService.FindByEmailAsync("test@StarterApp.com").Returns(user);
        _identityService.CheckPasswordAsync(user, "Password123").Returns(true);
        _tokenService.CreateTokenAsync(user).Returns("fake-jwt-token");

        var request = new LoginRequest { Email = "test@StarterApp.com", Password = "Password123" };

        var result = await _sut.LoginAsync(request);

        result.Status.Should().Be(LoginStatus.Success);
        result.Response.Should().NotBeNull();
        result.Response!.Token.Should().Be("fake-jwt-token");
        result.Response.Email.Should().Be("test@StarterApp.com");
        result.Response.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsInactive()
    {
        var user = new FakeIdentityUser { Email = "pending@StarterApp.com", IsActive = false };
        _identityService.FindByEmailAsync("pending@StarterApp.com").Returns(user);
        _identityService.CheckPasswordAsync(user, "Password123").Returns(true);

        var request = new LoginRequest { Email = "pending@StarterApp.com", Password = "Password123" };

        var result = await _sut.LoginAsync(request);

        result.Status.Should().Be(LoginStatus.Inactive);
        result.Response.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsInvalidCredentials()
    {
        _identityService.FindByEmailAsync("unknown@StarterApp.com").Returns((IIdentityUser?)null);

        var request = new LoginRequest { Email = "unknown@StarterApp.com", Password = "any" };

        var result = await _sut.LoginAsync(request);

        result.Status.Should().Be(LoginStatus.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        var user = new FakeIdentityUser { Email = "test@StarterApp.com" };
        _identityService.FindByEmailAsync("test@StarterApp.com").Returns(user);
        _identityService.CheckPasswordAsync(user, "wrong").Returns(false);

        var request = new LoginRequest { Email = "test@StarterApp.com", Password = "wrong" };

        var result = await _sut.LoginAsync(request);

        result.Status.Should().Be(LoginStatus.InvalidCredentials);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateFails_ReturnsFalse()
    {
        _identityService.CreateUserAsync("new@StarterApp.com", "New User", "Password123").Returns(false);

        var request = new RegisterRequest
        {
            Email = "new@StarterApp.com",
            FullName = "New User",
            Password = "Password123"
        };

        var result = await _sut.RegisterAsync(request);

        result.Should().BeFalse();
        await _userCompanyRepo.DidNotReceive().AddAsync(Arg.Any<UserCompany>());
        await _accountEmailSender.DidNotReceive()
            .SendNewRegistrationToAdminAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateSucceeds_AssignsNoneCompany()
    {
        var userId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        _identityService.CreateUserAsync("new@StarterApp.com", "New User", "Password123").Returns(true);
        _identityService.FindByEmailAsync("new@StarterApp.com")
            .Returns(new FakeIdentityUser { Id = userId.ToString(), Email = "new@StarterApp.com" });
        _companyRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<Company, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new Company { CompanyId = 11, Name = "None" });
        _userCompanyRepo.AnyAsync(Arg.Any<Expression<Func<UserCompany, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var request = new RegisterRequest
        {
            Email = "new@StarterApp.com",
            FullName = "New User",
            Password = "Password123"
        };

        var result = await _sut.RegisterAsync(request);

        result.Should().BeTrue();
        await _userCompanyRepo.Received(1)
            .AddAsync(Arg.Is<UserCompany>(uc => uc.UserId == userId && uc.CompanyId == 11));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _accountEmailSender.Received(1)
            .SendRegistrationReceivedAsync("new@StarterApp.com", "New User", Arg.Any<CancellationToken>());
        await _accountEmailSender.Received(1)
            .SendNewRegistrationToAdminAsync("new@StarterApp.com", "New User", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordAsync_DelegatesToIdentityService()
    {
        _identityService.ChangePasswordAsync("test@StarterApp.com", "OldPass1", "NewPass1")
            .Returns(new OperationResult { Success = true });

        var request = new ChangePasswordRequest
        {
            Email = "test@StarterApp.com",
            CurrentPassword = "OldPass1",
            NewPassword = "NewPass1"
        };

        var result = await _sut.ChangePasswordAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsToken_WhenUserExists()
    {
        _identityService.GeneratePasswordResetTokenAsync("test@StarterApp.com").Returns("reset-token");

        var request = new ForgotPasswordRequest { Email = "test@StarterApp.com" };

        var result = await _sut.ForgotPasswordAsync(request);

        result.Should().Be("reset-token");
    }

    [Fact]
    public async Task ResetPasswordAsync_DelegatesToIdentityService()
    {
        _identityService.ResetPasswordAsync("test@StarterApp.com", "token", "NewPass1")
            .Returns(new OperationResult { Success = true });

        var request = new ResetPasswordRequest
        {
            Email = "test@StarterApp.com",
            Token = "token",
            NewPassword = "NewPass1"
        };

        var result = await _sut.ResetPasswordAsync(request);

        result.Success.Should().BeTrue();
    }
}
