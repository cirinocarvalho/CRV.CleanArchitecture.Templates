using FluentAssertions;
using NSubstitute;
using StarterApp.Application.Interfaces;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Application.Services;
using StarterApp.Domain.Entities;
using Xunit;

namespace StarterApp.UnitTests.Services;

public class CompanyAdminServiceTests
{
    private readonly IRepositoryBase<Company> _companyRepo = Substitute.For<IRepositoryBase<Company>>();
    private readonly IRepositoryBase<UserCompany> _userCompanyRepo = Substitute.For<IRepositoryBase<UserCompany>>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CompanyAdminService _sut;

    public CompanyAdminServiceTests()
        => _sut = new CompanyAdminService(_companyRepo, _userCompanyRepo, _uow);

    [Fact]
    public async Task CreateAsync_NewName_AddsAndReturnsCompany()
    {
        _companyRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _companyRepo.AddAsync(Arg.Any<Company>())
            .Returns(ci => { var c = ci.Arg<Company>(); c.CompanyId = 7; return c; });

        var result = await _sut.CreateAsync("Verizon");

        result.Should().NotBeNull();
        result!.CompanyId.Should().Be(7);
        result.Name.Should().Be("Verizon");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ReturnsNull()
    {
        _companyRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.CreateAsync("Verizon");

        result.Should().BeNull();
        await _companyRepo.DidNotReceive().AddAsync(Arg.Any<Company>());
    }

    [Fact]
    public async Task DeleteAsync_CompanyInUse_ReturnsInUse()
    {
        _companyRepo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(new Company { CompanyId = 5, Name = "X" });
        _userCompanyRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<UserCompany, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.DeleteAsync(5);

        result.Should().Be(CompanyMutationResult.InUse);
        await _companyRepo.DidNotReceive().DeleteAsync(Arg.Any<Company>());
    }

    [Fact]
    public async Task DeleteAsync_NotInUse_Deletes()
    {
        _companyRepo.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(new Company { CompanyId = 5, Name = "X" });
        _userCompanyRepo.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<UserCompany, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.DeleteAsync(5);

        result.Should().Be(CompanyMutationResult.Success);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
