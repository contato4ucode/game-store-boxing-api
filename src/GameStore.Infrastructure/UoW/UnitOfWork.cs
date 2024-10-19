using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameStore.Infrastructure.UoW;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _dataContext;
    private IDbContextTransaction _transaction;

    public IBoxRepository Boxes {  get; }
    public IOrderRepository Orders { get; }
    public IProductRepository Products { get; }

    public UnitOfWork(DataContext dataContext,
        IBoxRepository boxRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _dataContext = dataContext;
        Boxes = boxRepository;
        Orders = orderRepository;
        Products = productRepository;
    }

    public async Task<int> SaveAsync()
    {
        return await _dataContext.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = await _dataContext.Database.BeginTransactionAsync();
        }
        return _transaction;
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dataContext.Dispose();
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
}
