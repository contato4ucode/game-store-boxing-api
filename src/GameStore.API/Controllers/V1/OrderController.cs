using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers.V1;

[Route("api/[controller]")]
public class OrderController : MainController
{
    private readonly IOrderService _orderService;

    public OrderController(INotifier notifier, IAspNetUser user, IOrderService orderService)
        : base(notifier, user)
    {
        _orderService = orderService;
    }

    [HttpPost("process-orders")]
    public async Task<IActionResult> ProcessOrders([FromBody] List<Order> orders)
    {
        if (orders == null || !orders.Any())
        {
            NotifyError("The order list cannot be empty.");
            return CustomResponse();
        }

        return await HandleRequestAsync(
            async () =>
            {
                var result = await _orderService.ProcessOrders(orders);
                return CustomResponse(result);
            },
            ex =>
            {
                HandleException(ex);
                return CustomResponse();
            }
        );
    }
}
