using AutoMapper;
using FluentAssertions;
using GameStore.API.Contracts.Requests;
using GameStore.API.Contracts.Responses;
using GameStore.API.Controllers.V1;
using GameStore.Domain.Common;
using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Models;
using GameStore.Domain.Models.ValueObjects;
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
        var orderResponse = new OrderDTO { Id = orderId };
        var cacheKey = $"Order:{orderId}";

        _redisCacheServiceMock.GetCacheValueAsync<OrderDTO>(cacheKey).Returns((OrderDTO)null);
        _orderServiceMock.GetOrderByIdAsync(orderId).Returns(order);
        _mapperMock.Map<OrderDTO>(order).Returns(orderResponse);

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
                new Product("Product 1", new Dimensions(10, 10, 10), 1.5, 100),
                new Product("Product 2", new Dimensions(5, 5, 5), 0.5, 50)
            })
        };

        var orderResponses = orders.Select(o => new OrderDTO
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            Products = o.Products.Select(p => new ProductDTO
            {
                Name = p.Name,
                Dimensions = p.Dimensions,
                Weight = p.Weight,
                Price = p.Price
            }).ToList()
        }).ToList();

        var cacheKey = "OrderList:Page:1:PageSize:10";
        var paginatedResponse = new PaginatedResponse<OrderDTO>(
            orderResponses,
            count: orderResponses.Count,
            pageNumber: 1,
            pageSize: 10
        );

        _redisCacheServiceMock
            .GetCacheValueAsync<PaginatedResponse<OrderDTO>>(cacheKey)
            .Returns((PaginatedResponse<OrderDTO>)null);

        _orderServiceMock.GetAllOrdersAsync().Returns(orders);
        _mapperMock.Map<IEnumerable<OrderDTO>>(orders).Returns(orderResponses);

        // Act
        var result = await controller.GetAllOrders(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));

        var data = response.GetType().GetProperty("data").GetValue(response) as PaginatedResponse<OrderDTO>;
        Assert.NotNull(data);
        Assert.Equal(paginatedResponse.Items, data.Items);

        await _redisCacheServiceMock.Received(1)
            .GetCacheValueAsync<PaginatedResponse<OrderDTO>>(cacheKey);

        await _redisCacheServiceMock.Received(1)
            .SetCacheValueWithPaginationAsync(
                cacheKey,
                Arg.Is<PaginatedResponse<OrderDTO>>(p =>
                    p.TotalItems == paginatedResponse.TotalItems &&
                    p.PageNumber == paginatedResponse.PageNumber &&
                    p.PageSize == paginatedResponse.PageSize &&
                    p.Items.SequenceEqual(paginatedResponse.Items)
                )
            );
    }

    [Fact]
    public async Task CreateOrder_ShouldReturn201_WhenOrderIsCreated()
    {
        // Arrange
        var request = new OrderRequest
        {
            CustomerId = Guid.NewGuid(),
            ProductIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            OrderDate = DateTime.UtcNow
        };

        var products = new List<Product>
        {
            new Product("Product 1", new Dimensions(10, 15, 20), 1.5, 100),
            new Product("Product 2", new Dimensions(5, 10, 10), 0.5, 50)
        };

        var order = new Order(request.CustomerId, request.OrderDate.Value, products);

        _orderServiceMock.CreateOrderAsync(request.CustomerId, request.ProductIds, Arg.Any<string>(), request.OrderDate)
            .Returns(order);

        _mapperMock.Map<OrderResponse>(order).Returns(new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Products = products.Select(p => new ProductResponse
            {
                Name = p.Name,
                Dimensions = p.Dimensions,
                Weight = p.Weight,
                Price = p.Price
            }).ToList()
        });

        // Act
        var result = await controller.CreateOrder(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenOrderCreationFails()
    {
        // Arrange
        var userEmail = "test@email";
        var orderDate = DateTime.UtcNow.AddDays(1);
        var orderRequest = new OrderRequest
        {
            CustomerId = Guid.NewGuid(),
            ProductIds = new List<Guid> { Guid.NewGuid() },
            OrderDate = orderDate
        };

        _orderServiceMock
            .CreateOrderAsync(orderRequest.CustomerId, orderRequest.ProductIds, userEmail, orderRequest.OrderDate)
            .Returns((Order)null);

        // Act
        var result = await controller.CreateOrder(orderRequest);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal("Order creation failed", response.GetType().GetProperty("errors").GetValue(response));
    }

    [Fact]
    public async Task CreateOrdersBulk_ShouldReturn201_WhenOrdersAreCreated()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var productId3 = Guid.NewGuid();

        var requests = new List<OrderRequest>
        {
            new OrderRequest
            {
                CustomerId = Guid.NewGuid(),
                ProductIds = new List<Guid> { productId1, productId2 },
                OrderDate = DateTime.UtcNow.AddHours(-1)
            },
            new OrderRequest
            {
                CustomerId = Guid.NewGuid(),
                ProductIds = new List<Guid> { productId3 },
                OrderDate = DateTime.UtcNow.AddHours(-2)
            }
        };

        var products = new List<Product>
        {
            new Product("Product 1", new Dimensions(10, 10, 10), 1.5, 100) { Id = productId1 },
            new Product("Product 2", new Dimensions(5, 5, 5), 0.5, 50) { Id = productId2 },
            new Product("Product 3", new Dimensions(8, 8, 8), 0.3, 30) { Id = productId3 }
        };

        var createdOrders = requests.Select(request => new Order(
            request.CustomerId,
            request.OrderDate ?? DateTime.UtcNow,
            request.ProductIds.Select(id => products.First(p => p.Id == id)).ToList()
        )).ToList();

        _orderServiceMock.CreateOrdersBulkAsync(Arg.Any<List<OrderDTO>>(), Arg.Any<string>())
            .Returns(createdOrders);

        var response = createdOrders.Select(o => new OrderResponse
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            OrderDate = o.OrderDate,
            Products = o.Products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Dimensions = p.Dimensions,
                Weight = p.Weight,
                Price = p.Price
            }).ToList()
        }).ToList();

        _mapperMock.Map<List<OrderResponse>>(createdOrders).Returns(response);

        // Act
        var result = await controller.CreateOrdersBulk(requests) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status201Created);

        var resultData = result.Value;
        Assert.NotNull(resultData);
        Assert.True((bool)resultData.GetType().GetProperty("success").GetValue(resultData));

        var data = resultData.GetType().GetProperty("data").GetValue(resultData) as List<OrderResponse>;
        Assert.NotNull(data);
        Assert.Equal(response.Count, data.Count);

        await _redisCacheServiceMock.Received(1).InvalidatePagedCacheAsync();
    }

    [Fact]
    public async Task CreateOrdersBulk_ShouldReturnBadRequest_WhenOrderCreationFails()
    {
        // Arrange
        var requests = new List<OrderRequest>
        {
            new OrderRequest
            {
                CustomerId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            },
            new OrderRequest
            {
                CustomerId = Guid.NewGuid(),
                ProductIds = new List<Guid> { Guid.NewGuid() },
                OrderDate = DateTime.UtcNow
            }
        };

        _orderServiceMock.CreateOrdersBulkAsync(Arg.Any<List<OrderDTO>>(), Arg.Any<string>())
            .Returns((List<Order>)null);

        // Act
        var result = await controller.CreateOrdersBulk(requests);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        Assert.NotNull(badRequestResult.Value);
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
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderRequest = new OrderRequest();
        var order = new Order { Id = orderId };
        var userEmail = "test@email";

        _mapperMock.Map<Order>(orderRequest).Returns(order);
        _orderServiceMock.UpdateOrderAsync(order, userEmail).Returns(true);

        // Act
        var result = await controller.UpdateOrder(orderId, orderRequest);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
