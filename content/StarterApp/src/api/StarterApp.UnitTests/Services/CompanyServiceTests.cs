using FluentAssertions;
using NSubstitute;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Application.Services;
using StarterApp.Domain.Entities;
using Xunit;

namespace StarterApp.UnitTests.Services;

public class CompanyServiceTests
{
    private readonly IReadRepositoryBase<Company> _companyRepo = Substitute.For<IReadRepositoryBase<Company>>();
    private readonly CompanyService _sut;

    public CompanyServiceTests()
        => _sut = new CompanyService(_companyRepo);

    [Fact]
    public async Task GetCompanyListAsync_ReturnsSortedNames()
    {
        _companyRepo.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Company { CompanyId = 1, Name = "Verizon" },
            new Company { CompanyId = 2, Name = "AT&T" }
        ]);

        var result = await _sut.GetCompanyListAsync();

        result.Should().ContainInOrder("AT&T", "Verizon");
    }
}
