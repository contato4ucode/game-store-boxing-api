using GameStore.Domain.Models;

namespace GameStore.Domain.Tests.Models;

public class OrderTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var products = new List<Product> { new Product("Product 1", 10, 10, 10, 1.0, 50.0m) };

        // Act
        var order = new Order(customerId, orderDate, products);

        // Assert
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(orderDate, order.OrderDate);
        Assert.Equal(products, order.Products);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenProductsAreNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Order(customerId, orderDate, null));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNoProductsProvided()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var products = new List<Product>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Order(customerId, orderDate, products));
        Assert.Equal("An order must contain at least one product.", exception.Message);
    }

    [Fact]
    public void TotalPrice_ShouldReturnCorrectSumOfProductPrices()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("Product 1", 10, 10, 10, 1.0, 50.0m),
            new Product("Product 2", 5, 5, 5, 0.5, 25.0m)
        };
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products);

        // Act
        var totalPrice = order.TotalPrice;

        // Assert
        Assert.Equal(75.0m, totalPrice);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var order = new Order();

        // Assert
        Assert.Equal(Guid.Empty, order.CustomerId);
        Assert.Empty(order.Products);
        Assert.Equal(default(DateTime), order.OrderDate);
    }

    [Fact]
    public void Id_ShouldBeGenerated_WhenOrderIsCreated()
    {
        // Act
        var order = new Order();

        // Assert
        Assert.NotEqual(Guid.Empty, order.Id);
    }

    [Fact]
    public void CreatedAt_ShouldBeSetToCurrentTime_WhenOrderIsCreated()
    {
        // Act
        var order = new Order();

        // Assert
        Assert.True((DateTime.UtcNow - order.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void ToggleIsDeleted_ShouldToggleIsDeletedFlag()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, new List<Product> { new Product("Product 1", 10, 10, 10, 1.0, 50.0m) });

        // Act
        order.ToggleIsDeleted();

        // Assert
        Assert.True(order.IsDeleted);

        // Act
        order.ToggleIsDeleted();

        // Assert
        Assert.False(order.IsDeleted);
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtToCurrentTime()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, new List<Product> { new Product("Product 1", 10, 10, 10, 1.0, 50.0m) });
        DateTime? previousUpdateTime = order.UpdatedAt;

        // Act
        order.Update();

        // Assert
        Assert.NotNull(order.UpdatedAt);
        Assert.True((DateTime.UtcNow - order.UpdatedAt.Value).TotalSeconds < 1);
        Assert.NotEqual(previousUpdateTime, order.UpdatedAt);
    }
}
