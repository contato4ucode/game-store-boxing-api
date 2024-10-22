using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IBoxService
{
    Task<Box?> GetByIdAsync(Guid boxId);
    Task<IEnumerable<Box>> GetAllAsync();
    Task<bool> CreateBoxAsync(Box box, string userEmail);
    Task<bool> UpdateBoxAsync(Box box, string userEmail);
    Task<bool> SoftDeleteBoxAsync(Guid boxId, string userEmail);
}
