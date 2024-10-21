using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Tests.Mappings;

public class EntityBaseMappingTests
{
    private readonly DbContextOptions<DataContext> _options;

    public EntityBaseMappingTests()
    {
        _options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "EntityBaseMappingTestDB")
            .Options;
    }

    [Fact]
    public void EntityBase_Should_Have_Correct_Configuration()
    {
        using var context = new DataContext(_options);
        var entityType = context.Model.FindEntityType(typeof(Product));

        // Assert
        Assert.NotNull(entityType);
        Assert.NotNull(entityType.FindProperty("Id"));
        Assert.NotNull(entityType.FindProperty("CreatedAt"));
        Assert.NotNull(entityType.FindProperty("CreatedByUser"));
    }
}
