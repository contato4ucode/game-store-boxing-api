using AutoMapper;
using GameStore.API.Contracts.Reponses;
using GameStore.API.Contracts.Requests;
using GameStore.API.Controllers.V1;
using GameStore.Domain.Common;
using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace GameStore.API.Tests.Controllers.V1;

public class OrderControllerTests : BaseControllerTests<OrderController>
{
    private readonly IOrderService _orderServiceMock;
    private readonly IMapper _mapperMock;
    private readonly IRedisCacheService _redisCacheServiceMock;

    public OrderControllerTests() : base()
    {
        _orderServiceMock = Substitute.For<IOrderService>();
        _mapperMock = Substitute.For<IMapper>();
        _redisCacheServiceMock = Substitute.For<IRedisCacheService>();

        controller = new OrderController(
            _orderServiceMock,
            _mapperMock,
            _redisCacheServiceMock,
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
    public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId };
        var orderResponse = new OrderResponse { Id = orderId };
        var cacheKey = $"Order:{orderId}";

        _redisCacheServiceMock.GetCacheValueAsync<OrderResponse>(cacheKey).Returns((OrderResponse)null);
        _orderServiceMock.GetOrderByIdAsync(orderId).Returns(order);
        _mapperMock.Map<OrderResponse>(order).Returns(orderResponse);

        // Act
        var result = await controller.GetOrderById(orderId);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(orderResponse, response.GetType().GetProperty("data").GetValue(response));

        // Assert
        await _redisCacheServiceMock.Received(1).SetCacheValueAsync(cacheKey, orderResponse);
    }

    [Fact]
    public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var cacheKey = $"Order:{orderId}";

        _redisCacheServiceMock.GetCacheValueAsync<OrderResponse>(cacheKey).Returns((OrderResponse)null);
        _orderServiceMock.GetOrderByIdAsync(orderId).Returns((Order)null);

        // Act
        var result = await controller.GetOrderById(orderId);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        // Assert
        Assert.NotNull(notFoundResult.Value);
        var response = notFoundResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal("Resource not found", response.GetType().GetProperty("errors").GetValue(response));
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnAllOrders_WhenOrdersExist()
    {
        // Arrange
        var orders = new List<Order>
    {
        new Order(Guid.NewGuid(), DateTime.UtcNow, new List<Product>
        {
            new Product("Product 1", 10, 10, 10, 1.5, 100),
            new Product("Product 2", 5, 5, 5, 0.5, 50)
        })
    };

        var orderResponses = orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            Products = o.Products.Select(p => new ProductResponse
            {
                Name = p.Name,
                Height = p.Height,
                Width = p.Width,
                Length = p.Length,
                Weight = p.Weight,
                Price = p.Price
            }).ToList()
        }).ToList();

        var cacheKey = "OrderList:Page:1:PageSize:10";

        var paginatedResponse = new PaginatedResponse<OrderResponse>(
            orderResponses,
            count: orderResponses.Count,
            pageNumber: 1,
            pageSize: 10
        );

        _redisCacheServiceMock
            .GetCacheValueAsync<PaginatedResponse<OrderResponse>>(cacheKey)
            .Returns((PaginatedResponse<OrderResponse>)null);

        _orderServiceMock.GetAllOrdersAsync().Returns(orders);
        _mapperMock.Map<IEnumerable<OrderResponse>>(orders).Returns(orderResponses);

        // Act
        var result = await controller.GetAllOrders(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));

        var data = response.GetType().GetProperty("data").GetValue(response) as PaginatedResponse<OrderResponse>;
        Assert.NotNull(data);
        Assert.Equal(paginatedResponse.Items, data.Items);

        await _redisCacheServiceMock.Received(1)
            .GetCacheValueAsync<PaginatedResponse<OrderResponse>>(cacheKey);

        await _redisCacheServiceMock.Received(1)
            .SetCacheValueAsync(
                cacheKey,
                Arg.Is<PaginatedResponse<OrderResponse>>(p =>
                    p.TotalItems == paginatedResponse.TotalItems &&
                    p.PageNumber == paginatedResponse.PageNumber &&
                    p.PageSize == paginatedResponse.PageSize &&
                    p.Items.SequenceEqual(paginatedResponse.Items)
                )
            );
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnCreated_WhenOrderIsValid()
    {
        // Arrange
        var orderRequest = new OrderRequest { CustomerId = Guid.NewGuid(), ProductIds = new List<Guid> { Guid.NewGuid() } };
        var order = new Order();
        var orderResponse = new OrderResponse();
        var userEmail = "test@email";

        _orderServiceMock.CreateOrderAsync(orderRequest.CustomerId, orderRequest.ProductIds, userEmail).Returns(order);
        _mapperMock.Map<OrderResponse>(order).Returns(orderResponse);

        // Act
        var result = await controller.CreateOrder(orderRequest);
        var createdResult = Assert.IsType<ObjectResult>(result);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var response = createdResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(orderResponse, response.GetType().GetProperty("data").GetValue(response));
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderCreationFails()
    {
        // Arrange
        var userEmail = "test@email";
        var orderRequest = new OrderRequest { CustomerId = Guid.NewGuid(), ProductIds = new List<Guid> { Guid.NewGuid() } };
        _orderServiceMock.CreateOrderAsync(orderRequest.CustomerId, orderRequest.ProductIds, userEmail).Returns((Order)null);

        // Act
        var result = await controller.CreateOrder(orderRequest);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal("Order creation failed", response.GetType().GetProperty("errors").GetValue(response));
    }

    [Fact]
    public async Task SoftDeleteOrder_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userEmail = "test@email";
        _orderServiceMock.SoftDeleteOrderAsync(orderId, userEmail).Returns(Task.FromResult(true));

        // Act
        var result = await controller.SoftDeleteOrder(orderId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _orderServiceMock.Received(1).SoftDeleteOrderAsync(orderId, userEmail);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDTO();
        var order = new Order { Id = orderId };
        var userEmail = "test@email";

        _mapperMock.Map<Order>(orderDto).Returns(order);
        _orderServiceMock.UpdateOrderAsync(order, userEmail).Returns(true);

        // Act
        var result = await controller.UpdateOrder(orderId, orderDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _orderServiceMock.Received(1).UpdateOrderAsync(order, userEmail);
    }
}
