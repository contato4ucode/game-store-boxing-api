using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IOrderService
{
    Task<Order?> CreateOrderAsync(Guid customerId, List<Guid> productIds, string userEmail);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<bool> UpdateOrderAsync(Order order, string userEmail);
    Task<bool> SoftDeleteOrderAsync(Guid orderId, string userEmail);
}
