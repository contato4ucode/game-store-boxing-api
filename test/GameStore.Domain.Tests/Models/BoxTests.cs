using GameStore.Domain.Models;

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

        // Act
        var box = new Box(name, height, width, length);

        // Assert
        Assert.Equal(name, box.Name);
        Assert.Equal(height, box.Height);
        Assert.Equal(width, box.Width);
        Assert.Equal(length, box.Length);
    }

    [Fact]
    public void Volume_ShouldReturnCorrectValue()
    {
        // Arrange
        var box = new Box("Medium Box", 10, 10, 10);

        // Act
        int volume = box.Volume;

        // Assert
        Assert.Equal(1000, volume);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var box = new Box("Large Box", 15, 25, 35);

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
        Assert.Null(box.Name);
        Assert.Equal(0, box.Height);
        Assert.Equal(0, box.Width);
        Assert.Equal(0, box.Length);
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
