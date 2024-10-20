using FluentValidation;

namespace GameStore.Domain.Models.Validations;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(o => o.CustomerId)
            .NotEmpty().WithMessage("Customer ID must be provided.");

        RuleFor(o => o.OrderDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Order date cannot be in the future.");

        RuleFor(o => o.Products)
            .NotEmpty().WithMessage("An order must contain at least one product.")
            .ForEach(product =>
            {
                product.SetValidator(new ProductValidator());
            });
    }
}
