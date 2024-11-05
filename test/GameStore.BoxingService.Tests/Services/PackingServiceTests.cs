using GameStore.BoxingService.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.Domain.Models.ValueObjects;
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
            new Product("P1", new Dimensions(10, 10, 10), 1, 10.0m),
            new Product("P2", new Dimensions(5, 5, 5), 1, 5.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", new Dimensions(20, 20, 20)),
            new Box("Box2", new Dimensions(15, 15, 15))
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
            new Product("P1", new Dimensions(50, 50, 50), 10, 100.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", new Dimensions(20, 20, 20))
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
            new Product("P1", new Dimensions(10, 10, 10), 1, 10.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", new Dimensions(20, 20, 20)),
            new Box("Box2", new Dimensions(15, 15, 15))
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

    [Fact]
    public async Task ProcessOrdersAsync_Should_Allocate_Products_To_Boxes_For_All_Orders()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var orders = new List<Order>
    {
        new Order(orderIds[0], DateTime.UtcNow, new List<Product>
        {
            new Product("P1", new Dimensions(10, 10, 10), 1, 10.0m),
            new Product("P2", new Dimensions(5, 5, 5), 1, 5.0m)
        }),
        new Order(orderIds[1], DateTime.UtcNow, new List<Product>
        {
            new Product("P3", new Dimensions(15, 15, 15), 1, 20.0m)
        })
    };

        var boxes = new List<Box>
    {
        new Box("Box1", new Dimensions(20, 20, 20)),
        new Box("Box2", new Dimensions(15, 15, 15))
    };

        _unitOfWork.Orders.GetById(orderIds[0]).Returns(orders[0]);
        _unitOfWork.Orders.GetById(orderIds[1]).Returns(orders[1]);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orderIds);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, response => Assert.NotEmpty(response.Boxes));
        Assert.Contains(result, r => r.OrderId == orderIds[0]);
        Assert.Contains(result, r => r.OrderId == orderIds[1]);
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Return_Empty_Allocation_When_No_Box_Fits_Products()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var orders = new List<Order>
        {
            new Order(orderIds[0], DateTime.UtcNow, new List<Product>
            {
                new Product("P1", new Dimensions(50, 50, 50), 10, 100.0m)
            }),
            new Order(orderIds[1], DateTime.UtcNow, new List<Product>
            {
                new Product("P2", new Dimensions(60, 60, 60), 15, 150.0m)
            })
        };

        var boxes = new List<Box>
        {
            new Box("Box1", new Dimensions(20, 20, 20))
        };

        _unitOfWork.Orders.GetById(orderIds[0]).Returns(orders[0]);
        _unitOfWork.Orders.GetById(orderIds[1]).Returns(orders[1]);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orderIds);

        // Assert
        Assert.Equal(2, result.Count);
        foreach (var response in result)
        {
            var allocation = response.Boxes.First();
            Assert.Null(allocation.BoxId);
            Assert.Contains("Produto não cabe em nenhuma caixa disponível.", allocation.Observation);
        }
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Return_Error_For_NonExistent_Orders()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWork.Orders.GetById(orderIds[0]).Returns((Order)null);
        _unitOfWork.Orders.GetById(orderIds[1]).Returns((Order)null);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orderIds);

        // Assert
        Assert.Equal(2, result.Count);
        foreach (var response in result)
        {
            var allocation = response.Boxes.First();
            Assert.Null(allocation.BoxId);
            Assert.Empty(allocation.Products);
            Assert.Equal("Order not found or contains no products.", allocation.Observation);
        }
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Handle_Mixed_Found_And_NotFound_Orders()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var foundOrder = new Order(orderIds[0], DateTime.UtcNow, new List<Product>
        {
            new Product("P1", new Dimensions(10, 10, 10), 1, 10.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Box1", new Dimensions(20, 20, 20))
        };

        _unitOfWork.Orders.GetById(orderIds[0]).Returns(foundOrder);
        _unitOfWork.Orders.GetById(orderIds[1]).Returns((Order)null);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orderIds);

        // Assert
        Assert.Equal(2, result.Count);

        var foundResponse = result.First(r => r.OrderId == orderIds[0]);
        Assert.NotEmpty(foundResponse.Boxes);
        Assert.Equal("Box1", foundResponse.Boxes.First().BoxId);

        var notFoundResponse = result.First(r => r.OrderId == orderIds[1]);
        var allocation = notFoundResponse.Boxes.First();
        Assert.Null(allocation.BoxId);
        Assert.Equal("Order not found or contains no products.", allocation.Observation);
    }
}
