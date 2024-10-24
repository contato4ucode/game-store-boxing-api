using FluentValidation;
using GameStore.Domain.Interfaces.UoW;

namespace GameStore.Domain.Models.Validations;

public class OrderValidator : AbstractValidator<Order>
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void ConfigureRulesForCreate()
    {
        RuleFor(o => o.CustomerId)
            .NotEmpty().WithMessage("Customer ID must be provided.")
            .WithMessage("The specified Customer ID does not exist.");

        RuleFor(o => o.OrderDate)
            .NotEmpty().WithMessage("Order date cannot be null.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Order date cannot be in the future.");

        RuleFor(o => o.Products)
            .NotEmpty().WithMessage("An order must contain at least one product.")
            .ForEach(product =>
            {
                product.SetValidator(new ProductValidator(_unitOfWork));
            });

        ConfigureCommonRules();
    }

    public void ConfigureRulesForUpdate(Order existingOrder)
    {
        RuleFor(o => o.CustomerId)
            .NotEmpty().WithMessage("Customer ID must be provided.")
            .WithMessage("The specified Customer ID does not exist.")
            .When(o => existingOrder.CustomerId != o.CustomerId);

        RuleFor(o => o.OrderDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Order date cannot be in the future.")
            .NotEmpty().WithMessage("Order date cannot be null.");

        RuleFor(o => o.Products)
            .NotEmpty().WithMessage("An order must contain at least one product.")
            .ForEach(product =>
            {
                product.SetValidator(new ProductValidator(_unitOfWork));
            });

        ConfigureCommonRules();
    }

    private void ConfigureCommonRules()
    {
    }
}
