using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using GameStore.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Tests.Context;

public class DataContextTests
{
    private readonly DbContextOptions<DataContext> _options;

    public DataContextTests()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "DataContextTestDB")
            .Options;
    }

    [Fact]
    public void DataContext_Should_Initialize_Without_Errors()
    {
        // Act & Assert
        using var context = new DataContext(_options);
        Assert.NotNull(context);
    }

    [Fact]
    public void DataContext_Should_Contain_Required_DbSets()
    {
        using var context = new DataContext(_options);

        Assert.NotNull(context.Products);
        Assert.NotNull(context.Orders);
        Assert.NotNull(context.Boxes);
    }

    [Fact]
    public async Task DataContext_Should_Save_And_Retrieve_Products()
    {
        using var context = new DataContext(_options);

        // Arrange
        var product = EntityTestHelper.CreateTestProduct();

        // Act
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var savedProduct = await context.Products.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(savedProduct);
        Assert.Equal("Test Product", savedProduct.Name);
        Assert.Equal(10, savedProduct.Height);
        Assert.Equal(99.99m, savedProduct.Price);
        Assert.Equal("TestUser", savedProduct.CreatedByUser);
    }

    [Fact]
    public async Task DataContext_Should_Save_And_Retrieve_Orders()
    {
        using var context = new DataContext(_options);

        // Arrange
        var product = EntityTestHelper.CreateTestProduct();
        var order = EntityTestHelper.CreateTestOrder(Guid.NewGuid(), new List<Product> { product });

        // Act
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var savedOrder = await context.Orders.Include(o => o.Products).FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(savedOrder);
        Assert.Single(savedOrder.Products);
        Assert.Equal(99.99m, savedOrder.TotalPrice);
        Assert.Equal("TestUser", savedOrder.CreatedByUser);
    }

    [Fact]
    public async Task DataContext_Should_Save_And_Retrieve_Boxes()
    {
        using var context = new DataContext(_options);

        // Arrange
        var box = EntityTestHelper.CreateTestBox();

        // Act
        context.Boxes.Add(box);
        await context.SaveChangesAsync();

        var savedBox = await context.Boxes.FirstOrDefaultAsync();

        // Assert
        Assert.NotNull(savedBox);
        Assert.Equal("Test Box", savedBox.Name);
        Assert.Equal(50, savedBox.Height);
        Assert.Equal("TestUser", savedBox.CreatedByUser);
    }
}
