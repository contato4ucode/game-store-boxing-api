using GameStore.Domain.DTOs;
using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<Order?> GetOrderByIdAsync(Guid orderId);
    Task<Order?> CreateOrderAsync(Guid customerId, List<Guid> productIds, string userEmail, DateTime? orderDate);
    Task<IEnumerable<Order>> CreateOrdersBulkAsync(List<OrderDTO> orderDtos, string userEmail);
    Task<bool> UpdateOrderAsync(Order order, string userEmail);
    Task<bool> SoftDeleteOrderAsync(Guid orderId, string userEmail);
}
