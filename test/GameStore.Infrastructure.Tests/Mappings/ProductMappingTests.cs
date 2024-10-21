using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using GameStore.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Tests.Mappings;

public class ProductMappingTests
{
    private readonly DbContextOptions<DataContext> _options;

    public ProductMappingTests()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "ProductMappingTestDB")
            .Options;
    }

    [Fact]
    public void Product_Table_Configuration_Should_Be_Valid()
    {
        using var context = new DataContext(_options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(Product));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("Products", entityType.GetTableName());
        Assert.NotNull(entityType.FindIndex(entityType.FindProperty("Name")));
    }

    [Fact]
    public async Task Product_Weight_And_Price_Should_Have_Precision()
    {
        using var context = new DataContext(_options);
        var product = EntityTestHelper.CreateTestProduct();

        // Act
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var savedProduct = await context.Products.FirstAsync();

        // Assert
        Assert.Equal(2.5, savedProduct.Weight);
        Assert.Equal(99.99m, savedProduct.Price);
    }
}
