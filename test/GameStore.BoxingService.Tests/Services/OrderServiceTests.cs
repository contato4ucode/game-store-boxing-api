using FluentValidation;
using FluentValidation.Results;
using GameStore.BoxingService.Services;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using NSubstitute;
using System.Linq.Expressions;

namespace GameStore.BoxingService.Tests.Services;

public class OrderServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotifier _notifier;
    private readonly IValidator<Order> _orderValidator;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _notifier = Substitute.For<INotifier>();
        _orderValidator = Substitute.For<IValidator<Order>>();
        _orderService = new OrderService(_unitOfWork, _notifier, _orderValidator);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Return_Order_When_Valid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new List<Guid> { Guid.NewGuid() };
        var products = new List<Product>
        {
            new Product("Test Product", 10, 10, 10, 2.0, 50.0m)
        };
        var userEmail = "test@email";

        _unitOfWork.Products.Find(Arg.Any<Expression<Func<Product, bool>>>())
            .Returns(Task.FromResult(products.AsEnumerable()));

        _orderValidator.ValidateAsync(Arg.Any<Order>())
            .Returns(Task.FromResult(new ValidationResult()));

        // Act
        var result = await _orderService.CreateOrderAsync(customerId, productIds, userEmail);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        await _unitOfWork.Orders.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Return_Null_When_Invalid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new List<Guid> { Guid.NewGuid() };
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var order = new Order(customerId, DateTime.UtcNow, products);
        var userEmail = "test@email";

        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("OrderDate", "Order date is invalid.")
        });

        _unitOfWork.Products.Find(Arg.Any<Expression<Func<Product, bool>>>()).Returns(products);
        _orderValidator.ValidateAsync(Arg.Any<Order>()).Returns(validationResult);

        // Act
        var result = await _orderService.CreateOrderAsync(customerId, productIds, userEmail);

        // Assert
        Assert.Null(result);
        _notifier.Received(1).NotifyValidationErrors(Arg.Is<ValidationResult>(v =>
            v.Errors.Any(e => e.ErrorMessage == "Order date is invalid.")));
    }

    [Fact]
    public async Task GetOrderByIdAsync_Should_Return_Order_When_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };

        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products)
        {
            Id = orderId
        };

        _unitOfWork.Orders.GetById(orderId).Returns(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task GetOrderByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _unitOfWork.Orders.GetById(orderId).Returns((Order)null);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_Should_Return_All_Orders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order(Guid.NewGuid(), DateTime.UtcNow, new List<Product>
            {
                new Product("Test Product 1", 10, 10, 10, 1.0, 50.0m)
            }),
            new Order(Guid.NewGuid(), DateTime.UtcNow, new List<Product>
            {
                new Product("Test Product 2", 20, 20, 20, 2.0, 100.0m)
            })
        };

        _unitOfWork.Orders.GetAll().Returns(orders);

        // Act
        var result = await _orderService.GetAllOrdersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateOrderAsync_Should_Return_True_When_Valid()
    {
        // Arrange
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products);
        var userEmail = "test@email";
        _orderValidator.ValidateAsync(order).Returns(new ValidationResult());

        // Act
        var result = await _orderService.UpdateOrderAsync(order, userEmail);

        // Assert
        Assert.True(result);
        await _unitOfWork.Orders.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task UpdateOrderAsync_Should_Return_False_When_Invalid()
    {
        // Arrange
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products);
        var userEmail = "test@email";
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("CustomerId", "Customer ID is required.")
        });
        _orderValidator.ValidateAsync(order).Returns(validationResult);

        // Act
        var result = await _orderService.UpdateOrderAsync(order, userEmail);

        // Assert
        Assert.False(result);
        _notifier.Received(1).NotifyValidationErrors(validationResult);
    }

    [Fact]
    public async Task SoftDeleteOrderAsync_Should_Return_True_When_Order_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products);
        var userEmail = "test@email";
        _unitOfWork.Orders.GetById(orderId).Returns(order);

        // Act
        var result = await _orderService.SoftDeleteOrderAsync(orderId, userEmail);

        // Assert
        Assert.True(result);
        Assert.True(order.IsDeleted);
        await _unitOfWork.Orders.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task SoftDeleteOrderAsync_Should_Return_False_When_Order_Not_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userEmail = "test@email";
        _unitOfWork.Orders.GetById(orderId).Returns((Order)null);

        // Act
        var result = await _orderService.SoftDeleteOrderAsync(orderId, userEmail);

        // Assert
        Assert.False(result);
        _notifier.Received(1).Handle("Order not found.");
    }
}
