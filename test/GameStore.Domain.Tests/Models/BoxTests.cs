using GameStore.Domain.Models;
using GameStore.Domain.Models.ValueObjects;

namespace GameStore.Domain.Tests.Models;

public class BoxTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        string name = "Small Box";
        int height = 10;
        int width = 20;
        int length = 30;
        var dimensions = new Dimensions(height, width, length);

        // Act
        var box = new Box(name, dimensions);

        // Assert
        Assert.Equal(name, box.Name);
        Assert.Equal(dimensions.Height, box.Dimensions.Height);
        Assert.Equal(dimensions.Width, box.Dimensions.Width);
        Assert.Equal(dimensions.Length, box.Dimensions.Length);
    }

    [Fact]
    public void Volume_ShouldReturnCorrectValue()
    {
        // Arrange
        var dimensions = new Dimensions(10, 10, 10);
        var box = new Box("Medium Box", dimensions);

        // Act
        int volume = box.Volume;

        // Assert
        Assert.Equal(1000, volume);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var dimensions = new Dimensions(15, 25, 35);
        var box = new Box("Large Box", dimensions);

        // Act
        string result = box.ToString();

        // Assert
        Assert.Equal("Large Box - 15x25x35 cm", result);
    }

    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var box = new Box();

        // Assert
        Assert.Equal(string.Empty, box.Name);
        Assert.Equal(0, box.Dimensions.Height);
        Assert.Equal(0, box.Dimensions.Width);
        Assert.Equal(0, box.Dimensions.Length);
    }

    [Fact]
    public void Id_ShouldBeGenerated_WhenBoxIsCreated()
    {
        // Act
        var box = new Box();

        // Assert
        Assert.NotEqual(Guid.Empty, box.Id);
    }

    [Fact]
    public void CreatedAt_ShouldBeSetToCurrentTime_WhenBoxIsCreated()
    {
        // Act
        var box = new Box();

        // Assert
        Assert.True((DateTime.UtcNow - box.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void ToggleIsDeleted_ShouldToggleIsDeletedFlag()
    {
        // Arrange
        var box = new Box();

        // Act
        box.ToggleIsDeleted();

        // Assert
        Assert.True(box.IsDeleted);

        // Act
        box.ToggleIsDeleted();

        // Assert
        Assert.False(box.IsDeleted);
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtToCurrentTime()
    {
        // Arrange
        var box = new Box();
        DateTime? previousUpdateTime = box.UpdatedAt;

        // Act
        box.Update();

        // Assert
        Assert.NotNull(box.UpdatedAt);
        Assert.True((DateTime.UtcNow - box.UpdatedAt.Value).TotalSeconds < 1);
        Assert.NotEqual(previousUpdateTime, box.UpdatedAt);
    }
}
