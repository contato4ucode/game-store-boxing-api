using GameStore.Domain.DTOs;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers.V1;

[Route("api/[controller]")]
public class PackingController : MainController
{
    private readonly IPackingService _packingService;

    public PackingController(
        IPackingService packingService,
        INotifier notifier,
        IAspNetUser user)
        : base(notifier, user)
    {
        _packingService = packingService;
    }

    [HttpPost("process-orders")]
    public async Task<IActionResult> ProcessOrders([FromBody] List<OrderPackingRequestDTO> orders)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        return await HandleRequestAsync(
            async () =>
            {
                var result = await _packingService.ProcessOrdersAsync(orders);
                return CustomResponse(result);
            },
            ex =>
            {
                HandleException(ex);
                return StatusCode(500, new
                {
                    success = false,
                    errors = new List<string> { "An unexpected error occurred." }
                });
            });
    }
}
