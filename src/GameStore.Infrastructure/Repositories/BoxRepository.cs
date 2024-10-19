using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Models;
using GameStore.Infrastructure.Context;

namespace GameStore.Infrastructure.Repositories;

public class BoxRepository : Repository<Box>, IBoxRepository
{
    public BoxRepository(DataContext context, INotifier notifier) 
        : base(context, notifier) { }
}
