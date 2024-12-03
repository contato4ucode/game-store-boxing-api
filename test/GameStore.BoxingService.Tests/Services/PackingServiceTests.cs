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

        var allocatedProducts = result.Boxes.SelectMany(b => b.Products).ToList();
        Assert.Equal(order.Products.Select(p => p.Name).OrderBy(p => p), allocatedProducts.OrderBy(p => p));
    }

    [Fact]
    public async Task ProcessOrderAsync_Should_Return_Error_When_Order_Not_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _unitOfWork.Orders.GetById(orderId).Returns((Order)null);

        // Act
        var result = await _packingService.ProcessOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Single(result.Boxes);
        var observation = result.Boxes.First();
        Assert.Null(observation.BoxId);
        Assert.Empty(observation.Products);
        Assert.Equal("Order not found or contains no products.", observation.Observation);
    }

    [Fact]
    public async Task ProcessOrderAsync_Should_Handle_Unfit_Products()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(orderId, DateTime.UtcNow, new List<Product>
        {
            new Product("Oversized Product", new Dimensions(100, 100, 100), 1, 200.0m)
        });

        var boxes = new List<Box>
        {
            new Box("Small Box", new Dimensions(20, 20, 20))
        };

        _unitOfWork.Orders.GetById(orderId).Returns(order);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Boxes);
        var unfitBox = result.Boxes.First();
        Assert.Null(unfitBox.BoxId);
        Assert.Single(unfitBox.Products, "Oversized Product");
        Assert.Equal("Produto não cabe em nenhuma caixa disponível.", unfitBox.Observation);
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Allocate_Multiple_Orders()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var orders = new List<Order>
        {
            new Order(orderIds[0], DateTime.UtcNow, new List<Product>
            {
                new Product("P1", new Dimensions(10, 10, 10), 1, 10.0m)
            }),
            new Order(orderIds[1], DateTime.UtcNow, new List<Product>
            {
                new Product("P2", new Dimensions(15, 15, 15), 1, 20.0m)
            })
        };

        var boxes = new List<Box>
        {
            new Box("Small Box", new Dimensions(20, 20, 20))
        };

        _unitOfWork.Orders.GetById(Arg.Any<Guid>()).Returns(callInfo =>
            orders.FirstOrDefault(o => o.Id == (Guid)callInfo[0]));
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orderIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderIds.Count, result.Count);

        foreach (var response in result)
        {
            Assert.NotEmpty(response.Boxes);
        }
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Handle_Mixed_Valid_And_Invalid_Orders()
    {
        // Arrange
        var validOrderId = Guid.NewGuid();
        var invalidOrderId = Guid.NewGuid();

        var orders = new List<Order>
        {
            new Order(validOrderId, DateTime.UtcNow, new List<Product>
            {
                new Product("Valid Product", new Dimensions(10, 10, 10), 1, 10.0m)
            })
        };

        var boxes = new List<Box>
        {
            new Box("Small Box", new Dimensions(20, 20, 20))
        };

        _unitOfWork.Orders.GetById(validOrderId).Returns(orders.First());
        _unitOfWork.Orders.GetById(invalidOrderId).Returns((Order)null);
        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(new List<Guid> { validOrderId, invalidOrderId });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var validResponse = result.First(r => r.OrderId == validOrderId);
        Assert.NotEmpty(validResponse.Boxes);

        var invalidResponse = result.First(r => r.OrderId == invalidOrderId);
        Assert.Single(invalidResponse.Boxes);
        Assert.Equal("Order not found or contains no products.", invalidResponse.Boxes.First().Observation);
    }
}
