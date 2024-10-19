using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Models;
using GameStore.Domain.Notifications;
using GameStore.SharedServices.Services;

namespace GameStore.BoxingService.Services;

public class OrderService : BaseService, IOrderService
{
    private readonly IBoxRepository _boxRepository;

    public OrderService(IBoxRepository boxRepository, INotifier notifier)
        : base(notifier)
    {
        _boxRepository = boxRepository;
    }

    public async Task<IEnumerable<OrderResponse>> ProcessOrders(List<Order> orders)
    {
        try
        {
            var availableBoxes = await _boxRepository.GetAll();
            var responses = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var allocatedBoxes = AllocateProductsToBoxes(order.Products, availableBoxes);
                responses.Add(new OrderResponse(order.Id, allocatedBoxes));
            }

            return responses;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return Enumerable.Empty<OrderResponse>();
        }
    }

    private List<BoxAllocation> AllocateProductsToBoxes(List<Product> products, IEnumerable<Box> availableBoxes)
    {
        var allocations = new List<BoxAllocation>();

        foreach (var product in products)
        {
            var suitableBox = availableBoxes.FirstOrDefault(b =>
                b.Height >= product.Height &&
                b.Width >= product.Width &&
                b.Length >= product.Length);

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
