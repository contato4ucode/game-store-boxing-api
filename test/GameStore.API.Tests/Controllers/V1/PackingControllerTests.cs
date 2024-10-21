using GameStore.API.Controllers.V1;
using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace GameStore.API.Tests.Controllers.V1;

public class PackingControllerTests : BaseControllerTests<PackingController>
{
    private readonly IPackingService _packingServiceMock;

    public PackingControllerTests() : base()
    {
        _packingServiceMock = Substitute.For<IPackingService>();

        controller = new PackingController(
            _packingServiceMock,
            _notifierMock,
            _userMock)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Fact]
    public async Task ProcessOrders_ShouldReturnOk_WhenOrdersAreProcessedSuccessfully()
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

        var response = new List<OrderPackingResponseDTO>
        {
            new OrderPackingResponseDTO
            {
                OrderId = "Order1",
                Boxes = new List<BoxAllocationDTO>
                {
                    new BoxAllocationDTO
                    {
                        BoxId = "Box1",
                        Products = new List<string> { "P1" }
                    }
                }
            }
        };

        _packingServiceMock.ProcessOrdersAsync(orders).Returns(response);

        // Act
        var result = await controller.ProcessOrders(orders);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.NotNull(okResult.Value);
        var responseObject = okResult.Value;
        Assert.True((bool)responseObject.GetType().GetProperty("success").GetValue(responseObject));
        Assert.Equal(response, responseObject.GetType().GetProperty("data").GetValue(responseObject));
    }

    [Fact]
    public async Task ProcessOrders_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        controller.ModelState.AddModelError("OrderId", "OrderId is required");

        var orders = new List<OrderPackingRequestDTO>
        {
            new OrderPackingRequestDTO { OrderId = null }
        };

        // Act
        var result = await controller.ProcessOrders(orders);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        var errors = response.GetType().GetProperty("errors").GetValue(response) as List<string>;
        Assert.Contains("OrderId is required", errors);
    }
}
