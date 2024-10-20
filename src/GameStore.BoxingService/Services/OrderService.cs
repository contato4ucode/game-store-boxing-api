using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
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

    public async Task<IEnumerable<OrderResponse>> ProcessOrders(List<Order> orders)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var availableBoxes = await _unitOfWork.Boxes.GetAll();
            var responses = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var allocatedBoxes = OptimizeBoxAllocation(order.Products, availableBoxes);
                responses.Add(new OrderResponse(order.Id, allocatedBoxes));
            }

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitTransactionAsync();

            return responses;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            HandleException(ex);
            return Enumerable.Empty<OrderResponse>();
        }
    }

    private List<BoxAllocation> OptimizeBoxAllocation(List<Product> products, IEnumerable<Box> availableBoxes)
    {
        var allocations = new List<BoxAllocation>();

        var sortedProducts = products
            .OrderByDescending(p => p.Height * p.Width * p.Length)
            .ToList();

        foreach (var product in sortedProducts)
        {
            var suitableBox = availableBoxes
                .Where(b =>
                    b.Height >= product.Height &&
                    b.Width >= product.Width &&
                    b.Length >= product.Length)
                .OrderBy(b => b.Height * b.Width * b.Length)
                .FirstOrDefault();

            if (suitableBox == null)
            {
                _notifier.Handle($"No suitable box found for product with dimensions {product.Height}x{product.Width}x{product.Length}.", NotificationType.Error);
                continue;
            }

            var allocation = allocations.FirstOrDefault(a => a.Box.Name == suitableBox.Name);

            if (allocation == null)
            {
                allocation = new BoxAllocation(suitableBox);
                allocations.Add(allocation);
            }

            allocation.Products.Add(product);
        }

        return allocations;
    }
}
