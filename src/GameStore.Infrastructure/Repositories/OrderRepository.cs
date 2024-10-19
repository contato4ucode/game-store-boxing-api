using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;

namespace GameStore.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(DataContext context, INotifier notifier)
        : base(context, notifier) { }
}
