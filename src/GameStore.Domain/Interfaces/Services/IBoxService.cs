using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IBoxService
{
    Task<Box?> GetByIdAsync(Guid boxId);
    Task<IEnumerable<Box>> GetAllAsync();
    Task<bool> CreateBoxAsync(Box box);
    Task<bool> UpdateBoxAsync(Box box);
    Task<bool> SoftDeleteBoxAsync(Guid boxId);
}
