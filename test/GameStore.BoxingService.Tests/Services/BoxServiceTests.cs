using FluentValidation;
using FluentValidation.Results;
using GameStore.BoxingService.Services;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.Domain.Models.Validations;
using NSubstitute;

namespace GameStore.BoxingService.Tests.Services;

public class BoxServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotifier _notifier;
    private readonly BoxService _boxService;

    public BoxServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _notifier = Substitute.For<INotifier>();
        _boxService = new BoxService(_unitOfWork, _notifier);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Box_When_Found()
    {
        // Arrange
        var boxId = Guid.NewGuid();
        var box = new Box("Test Box", 50, 50, 50);
        _unitOfWork.Boxes.GetById(boxId).Returns(box);

        // Act
        var result = await _boxService.GetByIdAsync(boxId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Box", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var boxId = Guid.NewGuid();
        _unitOfWork.Boxes.GetById(boxId).Returns((Box)null);

        // Act
        var result = await _boxService.GetByIdAsync(boxId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Boxes()
    {
        // Arrange
        var boxes = new List<Box>
        {
            new Box("Box 1", 30, 40, 80),
            new Box("Box 2", 80, 50, 40)
        };
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _boxService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateBoxAsync_Should_Return_True_When_Valid()
    {
        // Arrange
        var box = new Box("Box 1", 30, 40, 80);
        var userEmail = "test@email";
        var validator = new BoxValidator(_unitOfWork);
        validator.ConfigureRulesForCreate();

        var validationResult = await validator.ValidateAsync(box);

        _unitOfWork.Boxes.Add(box).Returns(Task.CompletedTask);
        _unitOfWork.SaveAsync().Returns(Task.FromResult(1));

        // Act
        var result = await _boxService.CreateBoxAsync(box, userEmail);

        // Assert
        Assert.True(result);
        await _unitOfWork.Boxes.Received(1).Add(box);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task CreateBoxAsync_Should_Return_False_When_Invalid()
    {
        // Arrange
        var box = new Box("Invalid Box", -10, 40, 80);
        var userEmail = "test@email";
        var expectedErrorMessage = "Height must be greater than 0.";

        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("Height", expectedErrorMessage)
        });

        var validator = new BoxValidator(_unitOfWork);
        validator.ConfigureRulesForCreate();

        _notifier.When(x => x.NotifyValidationErrors(Arg.Is<ValidationResult>(v =>
            v.Errors.Any(e => e.ErrorMessage == expectedErrorMessage)))).Do(_ => { });

        // Act
        var result = await _boxService.CreateBoxAsync(box, userEmail);

        // Assert
        Assert.False(result);
        _notifier.Received(1).NotifyValidationErrors(Arg.Is<ValidationResult>(v =>
            v.Errors.Any(e => e.ErrorMessage == expectedErrorMessage)));
        await _unitOfWork.Boxes.DidNotReceive().Add(box);
    }

    [Fact]
    public async Task UpdateBoxAsync_Should_Return_True_When_Valid()
    {
        // Arrange
        var box = new Box("Box 1", 30, 40, 80) { Id = Guid.NewGuid() };
        var userEmail = "test@email";

        _unitOfWork.Boxes.GetById(box.Id).Returns(box);

        var validator = new BoxValidator(_unitOfWork);
        validator.ConfigureRulesForUpdate(box);

        var validationResult = new ValidationResult();
        _notifier.When(x => x.NotifyValidationErrors(Arg.Any<ValidationResult>())).Do(_ => { });

        // Act
        var result = await _boxService.UpdateBoxAsync(box, userEmail);

        // Assert
        Assert.True(result, "Expected the update operation to return true.");
        await _unitOfWork.Boxes.Received(1).Update(box);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task SoftDeleteBoxAsync_Should_Return_True_When_Box_Found()
    {
        // Arrange
        var boxId = Guid.NewGuid();
        var box = new Box("Box 1", 30, 40, 80);
        var userEmail = "test@email";

        _unitOfWork.Boxes.GetById(boxId).Returns(box);

        // Act
        var result = await _boxService.SoftDeleteBoxAsync(boxId, userEmail);

        // Assert
        Assert.True(result);
        Assert.True(box.IsDeleted);
        await _unitOfWork.Boxes.Received(1).Update(box);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task SoftDeleteBoxAsync_Should_Return_False_When_Box_Not_Found()
    {
        // Arrange
        var boxId = Guid.NewGuid();
        var userEmail = "test@email";
        _unitOfWork.Boxes.GetById(boxId).Returns((Box)null);

        // Act
        var result = await _boxService.SoftDeleteBoxAsync(boxId, userEmail);

        // Assert
        Assert.False(result);
        _notifier.Received(1).Handle("Box not found.");
    }
}
