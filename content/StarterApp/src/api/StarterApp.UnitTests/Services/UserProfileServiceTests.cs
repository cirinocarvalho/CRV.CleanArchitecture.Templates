using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Application.Services;
using StarterApp.Domain.Entities;
using Xunit;

namespace StarterApp.UnitTests.Services;

public class UserProfileServiceTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IReadRepositoryBase<UserCompany> _userCompanyRepo = Substitute.For<IReadRepositoryBase<UserCompany>>();
    private readonly IReadRepositoryBase<Company> _companyRepo = Substitute.For<IReadRepositoryBase<Company>>();
    private readonly UserProfileService _sut;

    private static readonly Guid UserGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public UserProfileServiceTests()
        => _sut = new UserProfileService(_identity, _userCompanyRepo, _companyRepo);

    [Fact]
    public async Task GetProfileByEmailAsync_ResolvesCompanyFromUserCompany()
    {
        var user = Substitute.For<IIdentityUser>();
        user.Id.Returns(UserGuid.ToString());
        user.Email.Returns("a@b.com");
        user.FullName.Returns("Ann B");
        user.Roles.Returns(["User"]);
        _identity.FindByEmailAsync("a@b.com").Returns(user);

        _userCompanyRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<UserCompany, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new UserCompany { UserId = UserGuid, CompanyId = 3 });
        _companyRepo.GetByIdAsync(3, Arg.Any<CancellationToken>())
            .Returns(new Company { CompanyId = 3, Name = "Verizon" });

        var result = await _sut.GetProfileByEmailAsync("a@b.com");

        result.Should().NotBeNull();
        result!.Company.Should().Be("Verizon");
        result.Email.Should().Be("a@b.com");
    }

    [Fact]
    public async Task GetProfileByEmailAsync_NoAssignment_EmptyCompany()
    {
        var user = Substitute.For<IIdentityUser>();
        user.Id.Returns(UserGuid.ToString());
        user.Email.Returns("a@b.com");
        user.FullName.Returns("Ann B");
        user.Roles.Returns([]);
        _identity.FindByEmailAsync("a@b.com").Returns(user);
        _userCompanyRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<UserCompany, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((UserCompany?)null);

        var result = await _sut.GetProfileByEmailAsync("a@b.com");

        result!.Company.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProfileByEmailAsync_UnknownUser_ReturnsNull()
    {
        _identity.FindByEmailAsync("none@b.com").Returns((IIdentityUser?)null);

        var result = await _sut.GetProfileByEmailAsync("none@b.com");

        result.Should().BeNull();
    }
}
