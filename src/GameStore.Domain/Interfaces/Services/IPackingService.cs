using GameStore.Domain.DTOs;

namespace GameStore.Domain.Interfaces.Services;

public interface IPackingService
{
    Task<List<OrderPackingResponseDTO>> ProcessOrdersAsync(List<OrderPackingRequestDTO> orders);
}
