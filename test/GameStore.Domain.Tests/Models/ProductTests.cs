using GameStore.Domain.Models;
using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Domain.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var name = "Test Product";
        var description = "A product description.";
        var height = 10;
        var width = 20;
        var length = 30;
        var weight = 2.5;
        var price = 100.0m;
        var dimensions = new Dimensions(height, width, length);

        // Act
        var product = new Product(name, dimensions, weight, price, description);

        // Assert
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(dimensions.Height, product.Dimensions.Height);
        Assert.Equal(dimensions.Width, product.Dimensions.Width);
        Assert.Equal(dimensions.Length, product.Dimensions.Length);
        Assert.Equal(weight, product.Weight);
        Assert.Equal(price, product.Price);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues_WhenDescriptionIsNotProvided()
    {
        // Arrange
        var name = "Test Product";
        var height = 10;
        var width = 20;
        var length = 30;
        var weight = 2.5;
        var price = 100.0m;
        var dimensions = new Dimensions(height, width, length);

        // Act
        var product = new Product(name, dimensions, weight, price);

        // Assert
        Assert.Equal(name, product.Name);
        Assert.Null(product.Description);
        Assert.Equal(dimensions.Height, product.Dimensions.Height);
        Assert.Equal(dimensions.Width, product.Dimensions.Width);
        Assert.Equal(dimensions.Length, product.Dimensions.Length);
        Assert.Equal(weight, product.Weight);
        Assert.Equal(price, product.Price);
    }

    [Fact]
    public void Volume_ShouldCalculateCorrectly()
    {
        // Arrange
        var dimensions = new Dimensions(10, 5, 2);
        var product = new Product("Test Product", dimensions, 1.0, 50.0m);

        // Act
        var volume = product.Volume;

        // Assert
        Assert.Equal(100, volume);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var dimensions = new Dimensions(10, 5, 2);
        var product = new Product("Test Product", dimensions, 1.0, 50.00m);

        // Act
        var result = product.ToString();

        // Assert
        Assert.Equal("Test Product - 10x5x2 cm, 1 kg, R$ 50,00", result);
    }

    [Fact]
    public void Id_ShouldBeGenerated_WhenProductIsCreated()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void CreatedAt_ShouldBeSetToCurrentTime_WhenProductIsCreated()
    {
        // Act
        var product = new Product();

        // Assert
        Assert.True((DateTime.UtcNow - product.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void ToggleIsDeleted_ShouldToggleIsDeletedFlag()
    {
        // Arrange
        var dimensions = new Dimensions(10, 5, 2);
        var product = new Product("Test Product", dimensions, 1.0, 50.0m);

        // Act
        product.ToggleIsDeleted();

        // Assert
        Assert.True(product.IsDeleted);

        // Act
        product.ToggleIsDeleted();

        // Assert
        Assert.False(product.IsDeleted);
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtToCurrentTime()
    {
        // Arrange
        var dimensions = new Dimensions(10, 5, 2);
        var product = new Product("Test Product", dimensions, 1.0, 50.0m);
        DateTime? previousUpdateTime = product.UpdatedAt;

        // Act
        product.Update();

        // Assert
        Assert.NotNull(product.UpdatedAt);
        Assert.True((DateTime.UtcNow - product.UpdatedAt.Value).TotalSeconds < 1);
        Assert.NotEqual(previousUpdateTime, product.UpdatedAt);
    }
}
