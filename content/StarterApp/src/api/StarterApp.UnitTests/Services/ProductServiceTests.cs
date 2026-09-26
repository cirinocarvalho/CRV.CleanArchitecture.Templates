using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using StarterApp.Application.DTOs;
using StarterApp.Application.Interfaces.Persistence;
using StarterApp.Application.Services;
using StarterApp.Domain.Entities;
using Xunit;

namespace StarterApp.UnitTests.Services;

/// <summary>
/// Unit tests for ProductService — the sample slice. Shows how a service that depends on
/// the repository and unit-of-work abstractions is tested without a database.
/// Delete these along with the rest of the Product slice.
/// </summary>
public class ProductServiceTests
{
    private readonly IRepositoryBase<Product> _repository = Substitute.For<IRepositoryBase<Product>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProductService _sut;

    /// <summary>
    /// Sets up mocked dependencies and creates the system under test.
    /// </summary>
    public ProductServiceTests()
        => _sut = new ProductService(_repository, _unitOfWork);

    [Fact]
    public async Task GetAllAsync_ReturnsProductsOrderedByName()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new Product { Id = 1, Sku = "B-1", Name = "Bolt", Price = 2m },
            new Product { Id = 2, Sku = "A-1", Name = "Anchor", Price = 5m }
        ]);

        var result = await _sut.GetAllAsync();

        result.Select(p => p.Name).Should().ContainInOrder("Anchor", "Bolt");
    }

    [Fact]
    public async Task CreateAsync_NewSku_PersistsAndReturnsProduct()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.CreateAsync(new ProductRequest
        {
            Sku = "A-1",
            Name = "Anchor",
            Price = 5m
        });

        result.Should().NotBeNull();
        result!.Sku.Should().Be("A-1");
        await _repository.Received(1).AddAsync(Arg.Is<Product>(p => p.Sku == "A-1"));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_DuplicateSku_ReturnsNullAndDoesNotPersist()
    {
        _repository.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.CreateAsync(new ProductRequest { Sku = "A-1", Name = "Anchor" });

        result.Should().BeNull();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_MissingProduct_ReturnsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _sut.UpdateAsync(99, new ProductRequest { Sku = "A-1", Name = "Anchor" });

        result.Should().Be(ProductMutationResult.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_SkuTakenByAnotherProduct_ReturnsDuplicateSku()
    {
        _repository.GetByIdAsync(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new Product { Id = 1, Sku = "A-1", Name = "Anchor" });
        _repository.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.UpdateAsync(1, new ProductRequest { Sku = "B-1", Name = "Anchor" });

        result.Should().Be(ProductMutationResult.DuplicateSku);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_RemovesAndSaves()
    {
        var product = new Product { Id = 1, Sku = "A-1", Name = "Anchor" };
        _repository.GetByIdAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.DeleteAsync(1);

        result.Should().Be(ProductMutationResult.Success);
        await _repository.Received(1).DeleteAsync(product);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
