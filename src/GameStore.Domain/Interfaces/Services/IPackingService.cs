using GameStore.Domain.DTOs;

namespace GameStore.Domain.Interfaces.Services;

public interface IPackingService
{
    Task<OrderPackingResponseDTO> ProcessOrderAsync(Guid orderId);
}
