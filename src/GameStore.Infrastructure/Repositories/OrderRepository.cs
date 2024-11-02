using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Models;
using GameStore.Domain.Notifications;
using GameStore.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(DataContext context, INotifier notifier)
        : base(context, notifier) { }

    public override async Task<Order> GetById(Guid id)
    {
        try
        {
            _notifier.Handle($"Getting Order by ID {id}.");
            return await _dbSet
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error getting Order by ID {id}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public override async Task<IEnumerable<Order>> GetAll()
    {
        try
        {
            _notifier.Handle($"Getting all Orders.");
            return await _dbSet
                .Include(o => o.Products)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error getting all Orders: {ex.Message}", NotificationType.Error);
            throw;
        }
    }
}
