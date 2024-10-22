using FluentValidation;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.SharedServices.Services;

namespace GameStore.BoxingService.Services;

public class BoxService : BaseService, IBoxService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<Box> _boxValidator;

    public BoxService(IUnitOfWork unitOfWork, INotifier notifier, IValidator<Box> boxValidator)
        : base(notifier)
    {
        _unitOfWork = unitOfWork;
        _boxValidator = boxValidator;
    }

    public async Task<Box?> GetByIdAsync(Guid boxId)
    {
        try
        {
            return await _unitOfWork.Boxes.GetById(boxId);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return null;
        }
    }

    public async Task<IEnumerable<Box>> GetAllAsync()
    {
        try
        {
            return await _unitOfWork.Boxes.GetAll();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return Enumerable.Empty<Box>();
        }
    }

    public async Task<bool> CreateBoxAsync(Box box, string userEmail)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var validationResult = await _boxValidator.ValidateAsync(box);
            if (!validationResult.IsValid)
            {
                _notifier.NotifyValidationErrors(validationResult);
                return false;
            }

            box.CreatedByUser = userEmail;

            await _unitOfWork.Boxes.Add(box);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            HandleException(ex);
            return false;
        }
    }

    public async Task<bool> UpdateBoxAsync(Box box, string userEmail)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var validationResult = await _boxValidator.ValidateAsync(box);
            if (!validationResult.IsValid)
            {
                _notifier.NotifyValidationErrors(validationResult);
                return false;
            }

            box.UpdatedByUser = userEmail;

            await _unitOfWork.Boxes.Update(box);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            HandleException(ex);
            return false;
        }
    }

    public async Task<bool> SoftDeleteBoxAsync(Guid boxId, string userEmail)
    {
        try
        {
            var box = await _unitOfWork.Boxes.GetById(boxId);
            if (box == null)
            {
                _notifier.Handle("Box not found.");
                return false;
            }

            box.UpdatedByUser = userEmail;

            box.ToggleIsDeleted();
            await _unitOfWork.Boxes.Update(box);
            await _unitOfWork.SaveAsync();

            return true;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return false;
        }
    }
}
