using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.SharedServices.Services;

namespace GameStore.BoxingService.Services;

public class OrderService : BaseService, IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork, INotifier notifier)
        : base(notifier)
    {
        _unitOfWork = unitOfWork;
    }
}
