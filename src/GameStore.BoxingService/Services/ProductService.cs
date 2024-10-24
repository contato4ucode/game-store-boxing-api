using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Services;
using GameStore.Domain.Interfaces.UoW;
using GameStore.Domain.Models;
using GameStore.Domain.Models.Validations;
using GameStore.Domain.Notifications;
using GameStore.SharedServices.Services;

namespace GameStore.BoxingService.Services;

public class ProductService : BaseService, IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork, INotifier notifier)
        : base(notifier)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Product?> GetByIdAsync(Guid productId)
    {
        try
        {
            return await _unitOfWork.Products.GetById(productId);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return null;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            return await _unitOfWork.Products.GetAll();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<bool> CreateProductAsync(Product product, string userEmail)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var validator = new ProductValidator(_unitOfWork);
            validator.ConfigureRulesForCreate();

            var validationResult = await validator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                _notifier.NotifyValidationErrors(validationResult);
                return false;
            }

            product.CreatedByUser = userEmail;

            await _unitOfWork.Products.Add(product);
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

    public async Task<bool> UpdateProductAsync(Product product, string userEmail)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existingProduct = await _unitOfWork.Products.GetById(product.Id);
            if (existingProduct == null)
            {
                _notifier.Handle("Product not found", NotificationType.Error);
                return false;
            }

            var validator = new ProductValidator(_unitOfWork);
            validator.ConfigureRulesForUpdate(existingProduct);

            var validationResult = await validator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                _notifier.NotifyValidationErrors(validationResult);
                return false;
            }

            product.UpdatedByUser = userEmail;

            await _unitOfWork.Products.Update(product);
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

    public async Task<bool> SoftDeleteProductAsync(Guid productId, string userEmail)
    {
        try
        {
            var product = await _unitOfWork.Products.GetById(productId);
            if (product == null)
            {
                _notifier.Handle("Product not found.");
                return false;
            }

            product.UpdatedByUser = userEmail;

            product.ToggleIsDeleted();
            await _unitOfWork.Products.Update(product);
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
