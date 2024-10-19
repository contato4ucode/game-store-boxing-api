using GameStore.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameStore.Domain.Interfaces.UoW;

public interface IUnitOfWork : IDisposable
{
    IBoxRepository Boxes { get; }
    IOrderRepository Orders { get; }
    IProductRepository Products { get; }
    Task<int> SaveAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
