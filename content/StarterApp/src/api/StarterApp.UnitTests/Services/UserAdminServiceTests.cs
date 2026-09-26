using FluentAssertions;
using NSubstitute;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Application.Services;
using StarterApp.Domain.Entities;
using Xunit;

namespace StarterApp.UnitTests.Services;

public class UserAdminServiceTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IRepositoryBase<Company> _companyRepo = Substitute.For<IRepositoryBase<Company>>();
    private readonly IRepositoryBase<UserCompany> _userCompanyRepo = Substitute.For<IRepositoryBase<UserCompany>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IAccountEmailSender _accountEmailSender = Substitute.For<IAccountEmailSender>();
    private readonly UserAdminService _sut;

    private static readonly Guid UserGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public UserAdminServiceTests()
        => _sut = new UserAdminService(_identity, _companyRepo, _userCompanyRepo, _uow, _accountEmailSender);

    [Fact]
    public async Task GetUsersAsync_JoinsRolesAndCompany()
    {
        var user = Substitute.For<IIdentityUser>();
        user.Id.Returns(UserGuid.ToString());
        user.Email.Returns("a@b.com");
        user.FullName.Returns("Ann B");
        user.Roles.Returns(["Admin"]);
        _identity.GetAllUsersAsync().Returns([user]);

        _userCompanyRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([new UserCompany { UserId = UserGuid, CompanyId = 3 }]);
        _companyRepo.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([new Company { CompanyId = 3, Name = "Verizon" }]);

        var result = await _sut.GetUsersAsync();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(UserGuid);
        result[0].Roles.Should().Contain("Admin");
        result[0].CompanyId.Should().Be(3);
        result[0].CompanyName.Should().Be("Verizon");
    }

    [Fact]
    public async Task SetCompanyAsync_UnknownCompany_ReturnsFalse()
    {
        _companyRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.SetCompanyAsync(UserGuid, 99);

        result.Should().BeFalse();
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetCompanyAsync_NewAssignment_AddsRow()
    {
        _companyRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _userCompanyRepo.GetByIdAsync(UserGuid, Arg.Any<CancellationToken>()).Returns((UserCompany?)null);

        var result = await _sut.SetCompanyAsync(UserGuid, 3);

        result.Should().BeTrue();
        await _userCompanyRepo.Received(1).AddAsync(Arg.Is<UserCompany>(uc => uc.UserId == UserGuid && uc.CompanyId == 3));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GrantAdminAsync_DelegatesToIdentity()
    {
        _identity.AddToRoleByIdAsync(UserGuid.ToString(), "Admin").Returns(true);

        var result = await _sut.GrantAdminAsync(UserGuid);

        result.Should().BeTrue();
        await _identity.Received(1).AddToRoleByIdAsync(UserGuid.ToString(), "Admin");
    }
}
