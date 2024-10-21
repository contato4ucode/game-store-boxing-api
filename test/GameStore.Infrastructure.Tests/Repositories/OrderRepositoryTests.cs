using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using GameStore.Infrastructure.Repositories;
using GameStore.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace GameStore.Infrastructure.Tests.Repositories;

public class OrderRepositoryTests
{
    private readonly DataContext _context;
    private readonly INotifier _notifier;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "GameStoreTestDB")
            .Options;

        _context = new DataContext(options);
        _notifier = Substitute.For<INotifier>();
        _repository = new OrderRepository(_context, _notifier);
    }

    [Fact]
    public async Task Add_Should_Add_Order_To_Database()
    {
        // Arrange
        var product = EntityTestHelper.CreateTestProduct();
        var order = EntityTestHelper.CreateTestOrder(Guid.NewGuid(), new List<Product> { product });

        // Act
        await _repository.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedOrder = await _repository.GetById(order.Id);
        Assert.NotNull(retrievedOrder);
    }

    [Fact]
    public async Task GetAll_Should_Return_All_Orders()
    {
        // Arrange
        var product1 = EntityTestHelper.CreateTestProduct();
        var order1 = EntityTestHelper.CreateTestOrder(Guid.NewGuid(), new List<Product> { product1 });
        var product2 = EntityTestHelper.CreateTestProduct();
        var order2 = EntityTestHelper.CreateTestOrder(Guid.NewGuid(), new List<Product> { product2 });
        await _repository.Add(order1);
        await _repository.Add(order2);
        await _context.SaveChangesAsync();

        // Act
        var orders = await _repository.GetAll();

        // Assert
        Assert.Equal(2, orders.Count());
    }

    [Fact]
    public async Task Delete_Should_SoftDelete_Order()
    {
        // Arrange
        var product = EntityTestHelper.CreateTestProduct();
        var order = EntityTestHelper.CreateTestOrder(Guid.NewGuid(), new List<Product> { product });
        await _repository.Add(order);
        await _context.SaveChangesAsync();

        // Act
        await _repository.Delete(order);
        await _context.SaveChangesAsync();

        // Assert
        var deletedOrder = await _repository.GetById(order.Id);
        Assert.True(deletedOrder.IsDeleted);
    }
}
