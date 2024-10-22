using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IProductService
{
    Task<Product?> GetByIdAsync(Guid productId);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<bool> CreateProductAsync(Product product, string userEmail);
    Task<bool> UpdateProductAsync(Product product, string userEmail);
    Task<bool> SoftDeleteProductAsync(Guid productId, string userEmail);
}
