using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Tests.Mappings;

public class BoxMappingTests
{
    private readonly DbContextOptions<DataContext> _options;

    public BoxMappingTests()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "BoxMappingTestDB")
            .Options;
    }

    [Fact]
    public void Box_Table_Configuration_Should_Be_Valid()
    {
        using var context = new DataContext(_options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(Box));

        // Assert
        Assert.NotNull(entityType);
        Assert.Equal("Boxes", entityType.GetTableName());
        Assert.NotNull(entityType.FindIndex(entityType.FindProperty("Name")));
    }
}
