using FluentValidation;
using FluentValidation.Results;
using GameStore.BoxingService.Services;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using NSubstitute;

namespace GameStore.BoxingService.Tests.Services;

public class ProductServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotifier _notifier;
    private readonly IValidator<Product> _productValidator;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _notifier = Substitute.For<INotifier>();
        _productValidator = Substitute.For<IValidator<Product>>();
        _productService = new ProductService(_unitOfWork, _notifier, _productValidator);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Product_When_Found()
    {
        // Arrange
        var product = new Product("Test Product", 10, 10, 10, 2.5, 100.0m);
        _unitOfWork.Products.GetById(product.Id).Returns(product);

        // Act
        var result = await _productService.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _unitOfWork.Products.GetById(productId).Returns((Product)null);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Products()
    {
        // Arrange
        var products = new List<Product>
            {
                new Product("Product 1", 10, 10, 10, 1.0, 50.0m),
                new Product("Product 2", 20, 20, 20, 2.0, 100.0m)
            };
        _unitOfWork.Products.GetAll().Returns(products);

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateProductAsync_Should_Return_True_When_Valid()
    {
        // Arrange
        var product = new Product("Valid Product", 10, 10, 10, 2.0, 100.0m);
        _productValidator.ValidateAsync(product).Returns(new ValidationResult());

        // Act
        var result = await _productService.CreateProductAsync(product);

        // Assert
        Assert.True(result);
        await _unitOfWork.Products.Received(1).Add(product);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task CreateProductAsync_Should_Return_False_When_Invalid()
    {
        // Arrange
        var product = new Product("Invalid Product", 10, 10, 10, 2.0, 100.0m);
        var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Product name is required.")
            });

        _productValidator.ValidateAsync(product).Returns(validationResult);

        // Act
        var result = await _productService.CreateProductAsync(product);

        // Assert
        Assert.False(result);
        _notifier.Received(1).NotifyValidationErrors(validationResult);
    }

    [Fact]
    public async Task UpdateProductAsync_Should_Return_True_When_Valid()
    {
        // Arrange
        var product = new Product("Updated Product", 10, 10, 10, 2.0, 100.0m);
        _productValidator.ValidateAsync(product).Returns(new ValidationResult());

        // Act
        var result = await _productService.UpdateProductAsync(product);

        // Assert
        Assert.True(result);
        await _unitOfWork.Products.Received(1).Update(product);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task UpdateProductAsync_Should_Return_False_When_Invalid()
    {
        // Arrange
        var product = new Product("Invalid Product", 10, 10, 10, 2.0, 100.0m);
        var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Price", "Price must be greater than zero.")
            });

        _productValidator.ValidateAsync(product).Returns(validationResult);

        // Act
        var result = await _productService.UpdateProductAsync(product);

        // Assert
        Assert.False(result);
        _notifier.Received(1).NotifyValidationErrors(validationResult);
    }

    [Fact]
    public async Task SoftDeleteProductAsync_Should_Return_True_When_Product_Found()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product("Product to Delete", 10, 10, 10, 1.0, 50.0m);
        _unitOfWork.Products.GetById(productId).Returns(product);

        // Act
        var result = await _productService.SoftDeleteProductAsync(productId);

        // Assert
        Assert.True(result);
        Assert.True(product.IsDeleted);
        await _unitOfWork.Products.Received(1).Update(product);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task SoftDeleteProductAsync_Should_Return_False_When_Product_Not_Found()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _unitOfWork.Products.GetById(productId).Returns((Product)null);

        // Act
        var result = await _productService.SoftDeleteProductAsync(productId);

        // Assert
        Assert.False(result);
        _notifier.Received(1).Handle("Product not found.");
    }
}
