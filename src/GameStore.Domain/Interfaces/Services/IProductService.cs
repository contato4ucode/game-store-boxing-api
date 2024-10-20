using GameStore.Domain.Models;

namespace GameStore.Domain.Interfaces.Services;

public interface IProductService
{
    Task<Product?> GetByIdAsync(Guid productId);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<bool> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> SoftDeleteProductAsync(Guid productId);
}
