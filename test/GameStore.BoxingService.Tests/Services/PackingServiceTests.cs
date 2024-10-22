using GameStore.BoxingService.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using NSubstitute;

namespace GameStore.BoxingService.Tests.Services;

public class PackingServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PackingService _packingService;

    public PackingServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _packingService = new PackingService(_unitOfWork);
    }

    [Fact]
    public async Task ProcessOrderAsync_Should_Allocate_Products_To_Boxes()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(orderId, DateTime.UtcNow, new List<Product>
        {
            new Product("P1", 10, 10, 10, 1, 10.0m),
            new Product("P2", 5, 5, 5, 1, 5.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", 20, 20, 20),
            new Box("Box2", 15, 15, 15)
        };

        _unitOfWork.Orders.GetById(orderId).Returns(order);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.NotEmpty(result.Boxes);
    }

    [Fact]
    public async Task ProcessOrderAsync_Should_Return_Empty_Allocation_When_No_Box_Fits_Product()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(orderId, DateTime.UtcNow, new List<Product>
        {
            new Product("P1", 50, 50, 50, 10, 100.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", 20, 20, 20)
        };

        _unitOfWork.Orders.GetById(orderId).Returns(order);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        var allocation = result.Boxes.First();
        Assert.Null(allocation.BoxId);
        Assert.Contains("P1", allocation.Products);
        Assert.Equal("Produto não cabe em nenhuma caixa disponível.", allocation.Observation);
    }

    [Fact]
    public async Task ProcessOrderAsync_Should_Throw_Exception_When_Order_Not_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _unitOfWork.Orders.GetById(orderId).Returns((Order)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _packingService.ProcessOrderAsync(orderId));
    }

    [Fact]
    public async Task ProcessOrderAsync_Should_Use_Smallest_Suitable_Box()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(orderId, DateTime.UtcNow, new List<Product>
        {
            new Product("P1", 10, 10, 10, 1, 10.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", 20, 20, 20),
            new Box("Box2", 15, 15, 15)
        };

        _unitOfWork.Orders.GetById(orderId).Returns(order);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        var allocation = result.Boxes.First();
        Assert.Equal("Box2", allocation.BoxId);
    }
}
