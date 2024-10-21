using GameStore.BoxingService.Services;
using GameStore.Domain.DTOs;
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
    public async Task ProcessOrdersAsync_Should_Allocate_Products_To_Boxes()
    {
        // Arrange
        var orders = new List<OrderPackingRequestDTO>
            {
                new OrderPackingRequestDTO
                {
                    OrderId = "Order1",
                    Products = new List<ProductRequestDTO>
                    {
                        new ProductRequestDTO
                        {
                            ProductId = "P1",
                            Dimensions = new DimensionsDTO { Height = 10, Width = 10, Length = 10 }
                        },
                        new ProductRequestDTO
                        {
                            ProductId = "P2",
                            Dimensions = new DimensionsDTO { Height = 5, Width = 5, Length = 5 }
                        }
                    }
                }
            };

        var boxes = new List<Box>
            {
                new Box("Box1", 20, 20, 20),
                new Box("Box2", 15, 15, 15)
            };

        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orders);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Order1", result.First().OrderId);
        Assert.NotEmpty(result.First().Boxes);
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Return_Empty_Allocation_When_No_Box_Fits_Product()
    {
        // Arrange
        var orders = new List<OrderPackingRequestDTO>
            {
                new OrderPackingRequestDTO
                {
                    OrderId = "Order1",
                    Products = new List<ProductRequestDTO>
                    {
                        new ProductRequestDTO
                        {
                            ProductId = "P1",
                            Dimensions = new DimensionsDTO { Height = 50, Width = 50, Length = 50 }
                        }
                    }
                }
            };

        var boxes = new List<Box>
            {
                new Box("Box1", 20, 20, 20)
            };

        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orders);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var allocation = result.First().Boxes.First();
        Assert.Null(allocation.BoxId);
        Assert.Equal("P1", allocation.Products.First());
        Assert.Equal("Produto não cabe em nenhuma caixa disponível.", allocation.Observation);
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Handle_Multiple_Orders()
    {
        // Arrange
        var orders = new List<OrderPackingRequestDTO>
            {
                new OrderPackingRequestDTO
                {
                    OrderId = "Order1",
                    Products = new List<ProductRequestDTO>
                    {
                        new ProductRequestDTO
                        {
                            ProductId = "P1",
                            Dimensions = new DimensionsDTO { Height = 10, Width = 10, Length = 10 }
                        }
                    }
                },
                new OrderPackingRequestDTO
                {
                    OrderId = "Order2",
                    Products = new List<ProductRequestDTO>
                    {
                        new ProductRequestDTO
                        {
                            ProductId = "P2",
                            Dimensions = new DimensionsDTO { Height = 15, Width = 15, Length = 15 }
                        }
                    }
                }
            };

        var boxes = new List<Box>
            {
                new Box("Box1", 20, 20, 20)
            };

        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orders);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.OrderId == "Order1");
        Assert.Contains(result, r => r.OrderId == "Order2");
    }

    [Fact]
    public async Task ProcessOrdersAsync_Should_Use_Smallest_Suitable_Box()
    {
        // Arrange
        var orders = new List<OrderPackingRequestDTO>
            {
                new OrderPackingRequestDTO
                {
                    OrderId = "Order1",
                    Products = new List<ProductRequestDTO>
                    {
                        new ProductRequestDTO
                        {
                            ProductId = "P1",
                            Dimensions = new DimensionsDTO { Height = 10, Width = 10, Length = 10 }
                        }
                    }
                }
            };

        var boxes = new List<Box>
            {
                new Box("Box1", 20, 20, 20),
                new Box("Box2", 15, 15, 15)
            };

        _unitOfWork.Boxes.GetAll().Returns(boxes);

        // Act
        var result = await _packingService.ProcessOrdersAsync(orders);

        // Assert
        Assert.NotNull(result);
        var allocation = result.First().Boxes.First();
        Assert.Equal("Box2", allocation.BoxId);
    }
}
