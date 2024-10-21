using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using GameStore.Infrastructure.Repositories;
using GameStore.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace GameStore.Infrastructure.Tests.Repositories;

public class ProductRepositoryTests
{
    private readonly DataContext _context;
    private readonly INotifier _notifier;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "GameStoreTestDB")
            .Options;

        _context = new DataContext(options);
        _notifier = Substitute.For<INotifier>();
        _repository = new ProductRepository(_context, _notifier);
    }

    [Fact]
    public async Task Add_Should_Add_Product_To_Database()
    {
        // Arrange
        var product = EntityTestHelper.CreateTestProduct();

        // Act
        await _repository.Add(product);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedProduct = await _repository.GetById(product.Id);
        Assert.NotNull(retrievedProduct);
        Assert.Equal(product.Name, retrievedProduct.Name);
    }

    [Fact]
    public async Task GetById_Should_Return_Correct_Product()
    {
        // Arrange
        var product = EntityTestHelper.CreateTestProduct();
        await _repository.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Name, result.Name);
    }

    [Fact]
    public async Task Delete_Should_SoftDelete_Product()
    {
        // Arrange
        var product = EntityTestHelper.CreateTestProduct();
        await _repository.Add(product);
        await _context.SaveChangesAsync();

        // Act
        await _repository.Delete(product);
        await _context.SaveChangesAsync();

        // Assert
        var deletedProduct = await _repository.GetById(product.Id);
        Assert.True(deletedProduct.IsDeleted);
    }
}
