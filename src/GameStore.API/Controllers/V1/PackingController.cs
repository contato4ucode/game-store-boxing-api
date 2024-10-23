using Asp.Versioning;
using GameStore.Domain.Interfaces;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/packing")]
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

    [HttpPost("process-order/{orderId:guid}")]
    public async Task<IActionResult> ProcessOrder(Guid orderId)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        return await HandleRequestAsync(
        async () =>
        {
            var result = await _packingService.ProcessOrderAsync(orderId);
            return CustomResponse(result);
        },
        ex =>
        {
            HandleException(ex);
            return StatusCode(500, new Dictionary<string, object>
            {
            { "success", false },
            { "errors", new List<string> { "An unexpected error occurred." } }
            });
        });
    }
}
