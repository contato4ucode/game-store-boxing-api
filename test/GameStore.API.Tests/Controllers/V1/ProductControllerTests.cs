using AutoMapper;
using GameStore.API.Contracts.Reponses;
using GameStore.API.Contracts.Requests;
using GameStore.API.Controllers.V1;
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
        var products = new List<Product> { new Product() };
        var productResponses = new List<ProductResponse> { new ProductResponse() };
        var cacheKey = "ProductList:All";

        _redisCacheServiceMock.GetCacheValueAsync<IEnumerable<ProductResponse>>(cacheKey).Returns((IEnumerable<ProductResponse>)null);
        _productServiceMock.GetAllAsync().Returns(products);
        _mapperMock.Map<IEnumerable<ProductResponse>>(products).Returns(productResponses);

        // Act
        var result = await controller.GetAllProducts();
        var okResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.NotNull(okResult.Value);
        var response = okResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(productResponses, response.GetType().GetProperty("data").GetValue(response));
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreated_WhenProductIsValid()
    {
        // Arrange
        var productRequest = new ProductRequest { Name = "Product 1", Price = 100 };
        var product = new Product();
        var productResponse = new ProductResponse();

        _mapperMock.Map<Product>(productRequest).Returns(product);
        _productServiceMock.CreateProductAsync(product).Returns(true);
        _mapperMock.Map<ProductResponse>(product).Returns(productResponse);

        // Act
        var result = await controller.CreateProduct(productRequest);
        var createdResult = Assert.IsType<ObjectResult>(result);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var response = createdResult.Value;
        Assert.True((bool)response.GetType().GetProperty("success").GetValue(response));
        Assert.Equal(productResponse, response.GetType().GetProperty("data").GetValue(response));
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenCreationFails()
    {
        // Arrange
        var productRequest = new ProductRequest { Name = "Product 1", Price = 100 };
        var product = new Product();

        _mapperMock.Map<Product>(productRequest).Returns(product);
        _productServiceMock.CreateProductAsync(product).Returns(false);

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

        _mapperMock.Map<Product>(productRequest).Returns(product);
        _productServiceMock.UpdateProductAsync(product).Returns(true);

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

        _productServiceMock.SoftDeleteProductAsync(productId).Returns(Task.FromResult(true));

        // Act
        var result = await controller.SoftDeleteProduct(productId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _productServiceMock.Received(1).SoftDeleteProductAsync(productId);
    }
}
