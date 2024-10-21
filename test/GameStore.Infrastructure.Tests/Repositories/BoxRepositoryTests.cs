using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;
using GameStore.Infrastructure.Repositories;
using GameStore.Infrastructure.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace GameStore.Infrastructure.Tests.Repositories;

public class BoxRepositoryTests
{
    private readonly DataContext _context;
    private readonly INotifier _notifier;
    private readonly BoxRepository _repository;

    public BoxRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "GameStoreTestDB")
            .Options;

        _context = new DataContext(options);
        _notifier = Substitute.For<INotifier>();
        _repository = new BoxRepository(_context, _notifier);
    }

    [Fact]
    public async Task Add_Should_Add_Box_To_Database()
    {
        // Arrange
        var box = EntityTestHelper.CreateTestBox();

        // Act
        await _repository.Add(box);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedBox = await _context.Boxes.FindAsync(box.Id);
        Assert.NotNull(retrievedBox);
        Assert.Equal(box.Name, retrievedBox.Name);
    }

    [Fact]
    public async Task GetById_Should_Return_Correct_Box()
    {
        // Arrange
        var box = EntityTestHelper.CreateTestBox();
        await _repository.Add(box);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(box.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(box.Name, result.Name);
    }

    [Fact]
    public async Task Delete_Should_SoftDelete_Box()
    {
        // Arrange
        var box = EntityTestHelper.CreateTestBox();
        await _repository.Add(box);
        await _context.SaveChangesAsync();

        // Act
        await _repository.Delete(box);
        await _context.SaveChangesAsync();

        // Assert
        var deletedBox = await _repository.GetById(box.Id);
        Assert.True(deletedBox.IsDeleted);
    }
}
