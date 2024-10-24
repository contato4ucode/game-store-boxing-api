﻿using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.Domain.Models.Validations;
using GameStore.Domain.Notifications;
using GameStore.SharedServices.Services;

namespace GameStore.BoxingService.Services;

public class OrderService : BaseService, IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork, INotifier notifier)
        : base(notifier)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            return await _unitOfWork.Orders.GetById(orderId);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return null;
        }
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        try
        {
            return await _unitOfWork.Orders.GetAll();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<Order?> CreateOrderAsync(Guid customerId, List<Guid> productIds, string userEmail, DateTime? orderDate = null)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var products = await LoadProductsByIdsAsync(productIds);

            var finalOrderDate = orderDate ?? DateTime.UtcNow;

            var order = new Order(customerId, finalOrderDate, products);

            var validator = new OrderValidator(_unitOfWork);
            validator.ConfigureRulesForCreate();

            var validationResult = await validator.ValidateAsync(order);

            if (!validationResult.IsValid)
            {
                _notifier.NotifyValidationErrors(validationResult);
                await _unitOfWork.RollbackTransactionAsync();
                return null;
            }

            order.CreatedByUser = userEmail;

            await _unitOfWork.Orders.Add(order);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitTransactionAsync();

            return order;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            HandleException(ex);
            return null;
        }
    }

    public async Task<bool> UpdateOrderAsync(Order order, string userEmail)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existingOrder = await _unitOfWork.Orders.GetById(order.Id);
            if (existingOrder ==  null)
            {
                _notifier.Handle("Order not found", NotificationType.Error);
                return false;
            }

            var validator = new OrderValidator(_unitOfWork);
            validator.ConfigureRulesForUpdate(existingOrder);

            var validationResult = await validator.ValidateAsync(order);
            if (!validationResult.IsValid)
            {
                _notifier.NotifyValidationErrors(validationResult);
                return false;
            }

            order.UpdatedByUser = userEmail;

            await _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            HandleException(ex);
            return false;
        }
    }

    public async Task<bool> SoftDeleteOrderAsync(Guid orderId, string userEmail)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetById(orderId);
            if (order == null)
            {
                _notifier.Handle("Order not found.");
                return false;
            }

            order.UpdatedByUser = userEmail;

            order.ToggleIsDeleted();
            await _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveAsync();

            return true;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return false;
        }
    }

    private async Task<List<Product>> LoadProductsByIdsAsync(IEnumerable<Guid> productIds)
    {
        var products = await _unitOfWork.Products.Find(p => productIds.Contains(p.Id));

        if (products == null || !products.Any())
            throw new ArgumentException("Invalid product IDs.");

        return products.ToList();
    }
}
