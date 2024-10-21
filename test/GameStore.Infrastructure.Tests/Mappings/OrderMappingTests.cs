using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using GameStore.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Tests.Mappings;

public class OrderMappingTests
{
    private readonly DbContextOptions<DataContext> _options;

    public OrderMappingTests()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "OrderMappingTestDB")
            .Options;
    }

    [Fact]
    public void Order_Table_Configuration_Should_Be_Valid()
    {
        using var context = new DataContext(_options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(Order));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("Orders", entityType.GetTableName());
    }

    [Fact]
    public async Task Order_Should_Cascade_Delete_Products()
    {
        using var context = new DataContext(_options);
        var product = EntityTestHelper.CreateTestProduct();
        var order = EntityTestHelper.CreateTestOrder(Guid.NewGuid(), new List<Product> { product });

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Act
        context.Orders.Remove(order);
        await context.SaveChangesAsync();

        // Assert
        var products = await context.Products.ToListAsync();
        Assert.Empty(products);
    }
}
