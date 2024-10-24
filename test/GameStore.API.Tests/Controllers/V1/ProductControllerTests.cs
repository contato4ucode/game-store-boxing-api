using AutoMapper;
using FluentAssertions;
using GameStore.API.Contracts.Reponses;
using GameStore.API.Contracts.Requests;
using GameStore.API.Controllers.V1;
using GameStore.Domain.Common;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace GameStore.API.Tests.Controllers.V1;

public class ProductControllerTests : BaseControllerTests<ProductController>
{
    private readonly IProductService _productServiceMock;
    private readonly IMapper _mapperMock;
    private readonly IRedisCacheService _redisCacheServiceMock;

    public ProductControllerTests() : base()
    {
        _productServiceMock = Substitute.For<IProductService>();
        _mapperMock = Substitute.For<IMapper>();
        _redisCacheServiceMock = Substitute.For<IRedisCacheService>();

        controller = new ProductController(
            _productServiceMock,
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
    public async Task GetProductById_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId };
        var productResponse = new ProductResponse { Id = productId };
        var cacheKey = $"Product:{productId}";

        _redisCacheServiceMock.GetCacheValueAsync<ProductResponse>(cacheKey).Returns((ProductResponse)null);
        _productServiceMock.GetByIdAsync(productId).Returns(product);
        _mapperMock.Map<ProductResponse>(product).Returns(productResponse);

        // Act
        var result = await controller.GetProductById(productId);
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.NotNull(okResult.Value);
        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(productResponse, response.GetType().GetProperty("data").GetValue(response));
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cacheKey = $"Product:{productId}";

        _redisCacheServiceMock.GetCacheValueAsync<ProductResponse>(cacheKey).Returns((ProductResponse)null);
        _productServiceMock.GetByIdAsync(productId).Returns((Product)null);

        // Act
        var result = await controller.GetProductById(productId);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        // Assert
        var response = notFoundResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal("Product not found", response.GetType().GetProperty("errors").GetValue(response));
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
    {
        new Product("Product 1", 10, 10, 10, 1.5, 100),
        new Product("Product 2", 5, 5, 5, 0.5, 50)
    };

        var productResponses = products.Select(p => new ProductResponse
        {
            Name = p.Name,
            Height = p.Height,
            Width = p.Width,
            Length = p.Length,
            Weight = p.Weight,
            Price = p.Price
        }).ToList();

        var cacheKey = "ProductList:Page:1:PageSize:20";

        var paginatedResponse = new PaginatedResponse<ProductResponse>(
            productResponses,
            count: productResponses.Count,
            pageNumber: 1,
            pageSize: 20
        );

        _redisCacheServiceMock
            .GetCacheValueAsync<PaginatedResponse<ProductResponse>>(cacheKey)
            .Returns((PaginatedResponse<ProductResponse>)null);

        _productServiceMock.GetAllAsync().Returns(products);
        _mapperMock.Map<IEnumerable<ProductResponse>>(products).Returns(productResponses);

        // Act
        var result = await controller.GetAllProducts(1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));

        var data = response.GetType().GetProperty("data").GetValue(response) as PaginatedResponse<ProductResponse>;
        Assert.NotNull(data);
        Assert.Equal(paginatedResponse.Items, data.Items);

        await _redisCacheServiceMock.Received(1)
            .GetCacheValueAsync<PaginatedResponse<ProductResponse>>(cacheKey);

        await _redisCacheServiceMock.Received(1)
            .SetCacheValueAsync(
                cacheKey,
                Arg.Is<PaginatedResponse<ProductResponse>>(p =>
                    p.TotalItems == paginatedResponse.TotalItems &&
                    p.PageNumber == paginatedResponse.PageNumber &&
                    p.PageSize == paginatedResponse.PageSize &&
                    p.Items.SequenceEqual(paginatedResponse.Items)
                )
            );
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn201_WhenProductIsCreated()
    {
        // Arrange
        var request = new ProductRequest
        {
            Name = "New Product",
            Height = 10,
            Width = 15,
            Length = 20,
            Weight = 1.5,
            Price = 99.99M
        };
        var product = new Product(request.Name, request.Height, request.Width,
                                  request.Length, request.Weight, request.Price);

        _mapperMock.Map<Product>(request).Returns(product);
        _productServiceMock.CreateProductAsync(product, Arg.Any<string>()).Returns(true);

        // Act
        var result = await controller.CreateProduct(request) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenCreationFails()
    {
        // Arrange
        var productRequest = new ProductRequest { Name = "Product 1", Price = 100 };
        var product = new Product();
        var userEmail = "test@email";

        _mapperMock.Map<Product>(productRequest).Returns(product);
        _productServiceMock.CreateProductAsync(product, userEmail).Returns(false);

        // Act
        var result = await controller.CreateProduct(productRequest);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        // Assert
        var response = badRequestResult.Value;
        Assert.False((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal("Failed to create product", response.GetType().GetProperty("errors").GetValue(response));
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productRequest = new ProductRequest { Name = "Updated Product", Price = 150 };
        var product = new Product { Id = productId };
        var userEmail = "test@email";

        _mapperMock.Map<Product>(productRequest).Returns(product);
        _productServiceMock.UpdateProductAsync(product, userEmail).Returns(true);

        // Act
        var result = await controller.UpdateProduct(productId, productRequest);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SoftDeleteProduct_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userEmail = "test@email";

        _productServiceMock.SoftDeleteProductAsync(productId, userEmail).Returns(Task.FromResult(true));

        // Act
        var result = await controller.SoftDeleteProduct(productId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
