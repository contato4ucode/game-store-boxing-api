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
    public async Task ProcessOrder_ShouldReturnOk_WhenOrderIsProcessedSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var response = new OrderPackingResponseDTO
        {
            OrderId = orderId,
            Boxes = new List<BoxAllocationDTO>
            {
                new BoxAllocationDTO
                {
                    BoxId = "Box1",
                    Products = new List<string> { "P1" }
                }
            }
        };

        _packingServiceMock.ProcessOrderAsync(orderId).Returns(response);

        // Act
        var result = await controller.ProcessOrder(orderId);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.NotNull(okResult.Value);
        var responseObject = okResult.Value;
        Assert.True((bool)responseObject.GetType().GetProperty("success").GetValue(responseObject));
        Assert.Equal(response, responseObject.GetType().GetProperty("data").GetValue(responseObject));
    }

    [Fact]
    public async Task ProcessOrder_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        controller.ModelState.AddModelError("OrderId", "OrderId is required");

        var orderId = Guid.Empty;

        // Act
        var result = await controller.ProcessOrder(orderId);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        var errors = response.GetType().GetProperty("errors").GetValue(response) as List<string>;
        Assert.Contains("OrderId is required", errors);
    }

    [Fact]
    public async Task ProcessOrder_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _packingServiceMock.When(x => x.ProcessOrderAsync(orderId))
                           .Do(x => throw new Exception("Unexpected error"));

        // Act
        var result = await controller.ProcessOrder(orderId);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);

        Assert.NotNull(errorResult.Value);

        var response = errorResult.Value as Dictionary<string, object>;
        Assert.NotNull(response);

        Assert.False((bool)response["success"]);

        var errors = response["errors"] as List<string>;
        Assert.NotNull(errors);
        Assert.Contains("An unexpected error occurred.", errors);
    }

    [Fact]
    public async Task ProcessOrders_ShouldReturnOk_WhenOrdersAreProcessedSuccessfully()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var expectedResponses = new List<OrderPackingResponseDTO>
        {
            new OrderPackingResponseDTO
            {
                OrderId = orderIds[0],
                Boxes = new List<BoxAllocationDTO>
                {
                    new BoxAllocationDTO { BoxId = "Box1", Products = new List<string> { "P1" } }
                }
            },
            new OrderPackingResponseDTO
            {
                OrderId = orderIds[1],
                Boxes = new List<BoxAllocationDTO>
                {
                    new BoxAllocationDTO { BoxId = "Box2", Products = new List<string> { "P2" } }
                }
            }
        };

        _packingServiceMock.ProcessOrdersAsync(orderIds).Returns(expectedResponses);

        // Act
        var result = await controller.ProcessOrders(orderIds);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task ProcessOrders_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        controller.ModelState.AddModelError("OrderIds", "Order IDs are required");

        // Act
        var result = await controller.ProcessOrders(new List<Guid>());
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));

        var errors = response.GetType().GetProperty("errors").GetValue(response) as List<string>;
        Assert.NotNull(errors);
        Assert.Contains("Order IDs are required", errors);
    }

    [Fact]
    public async Task ProcessOrders_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var orderIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _packingServiceMock.When(x => x.ProcessOrdersAsync(orderIds))
                           .Do(x => throw new Exception("Unexpected error"));

        // Act
        var result = await controller.ProcessOrders(orderIds);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);

        var response = errorResult.Value as Dictionary<string, object>;
        Assert.NotNull(response);

        Assert.False((bool)response["success"]);

        var errors = response["errors"] as List<string>;
        Assert.NotNull(errors);
        Assert.Contains("An unexpected error occurred.", errors);
    }
}
