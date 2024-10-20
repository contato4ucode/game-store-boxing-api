using GameStore.Domain.Interfaces;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
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
}
