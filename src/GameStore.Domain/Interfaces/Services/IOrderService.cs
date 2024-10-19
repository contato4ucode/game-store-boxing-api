using GameStore.Domain.DTOs;
using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderResponse>> ProcessOrders(List<Order> orders);
}
