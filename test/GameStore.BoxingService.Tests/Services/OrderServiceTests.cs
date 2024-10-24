using FluentValidation;
using FluentValidation.Results;
using GameStore.BoxingService.Services;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.Domain.Models.Validations;
using NSubstitute;
using System.Linq.Expressions;

namespace GameStore.BoxingService.Tests.Services;

public class OrderServiceTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotifier _notifier;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _notifier = Substitute.For<INotifier>();
        _orderService = new OrderService(_unitOfWork, _notifier);
    }

    [Fact]
    public async Task CreateOrderAsync_Should_Return_Null_When_Invalid()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productIds = new List<Guid> { Guid.NewGuid() };
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var userEmail = "test@email";
        var invalidOrderDate = DateTime.UtcNow.AddDays(1);

        _unitOfWork.Products.Find(Arg.Any<Expression<Func<Product, bool>>>()).Returns(products);

        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new ValidationFailure("OrderDate", "Order date cannot be in the future.")
        });

        _notifier.When(x => x.NotifyValidationErrors(Arg.Is<ValidationResult>(v =>
            v.Errors.Any(e => e.ErrorMessage == "Order date cannot be in the future.")))).Do(_ => { });

        var orderService = new OrderService(_unitOfWork, _notifier);

        // Act
        var result = await orderService.CreateOrderAsync(customerId, productIds, userEmail, invalidOrderDate);

        // Assert
        Assert.Null(result);
        _notifier.Received(1).NotifyValidationErrors(Arg.Is<ValidationResult>(v =>
            v.Errors.Any(e => e.ErrorMessage == "Order date cannot be in the future.")));
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
        var customOrderDate = DateTime.UtcNow.AddDays(-1);

        _unitOfWork.Products.Find(Arg.Any<Expression<Func<Product, bool>>>()).Returns(products);
        _unitOfWork.Orders.Add(Arg.Any<Order>()).Returns(Task.CompletedTask);

        var orderService = new OrderService(_unitOfWork, _notifier);

        // Act
        var result = await orderService.CreateOrderAsync(customerId, productIds, userEmail, customOrderDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(products.Count, result.Products.Count);
        Assert.Equal(customOrderDate, result.OrderDate);

        await _unitOfWork.Orders.Received(1).Add(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveAsync();
    }

    [Fact]
    public async Task GetOrderByIdAsync_Should_Return_Order_When_Found()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products) { Id = orderId };

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
    public async Task UpdateOrderAsync_Should_Return_True_When_Valid()
    {
        // Arrange
        var products = new List<Product> { new Product("Test Product", 10, 10, 10, 2.0, 50.0m) };
        var order = new Order(Guid.NewGuid(), DateTime.UtcNow, products);
        var userEmail = "test@email";

        _unitOfWork.Orders.GetById(order.Id).Returns(order);
        var validator = new OrderValidator(_unitOfWork);
        validator.ConfigureRulesForUpdate(order);
        var validationResult = await validator.ValidateAsync(order);

        // Act
        var result = await _orderService.UpdateOrderAsync(order, userEmail);

        // Assert
        Assert.True(result);
        await _unitOfWork.Orders.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveAsync();
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
