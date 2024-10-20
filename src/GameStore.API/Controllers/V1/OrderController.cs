using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class OrderController : MainController
{
    private readonly IOrderService _orderService;

    public OrderController(INotifier notifier, IAspNetUser user, IOrderService orderService)
        : base(notifier, user)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Processa uma lista de pedidos e determina a melhor forma de alocar os produtos nas caixas.
    /// </summary>
    /// <param name="orders">Lista de pedidos contendo produtos com dimensões específicas.</param>
    /// <returns>Retorna as caixas utilizadas para cada pedido.</returns>
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
                return CustomResponse(result, StatusCodes.Status200OK);
            },
            ex =>
            {
                HandleException(ex);
                return CustomResponse();
            }
        );
    }
}
