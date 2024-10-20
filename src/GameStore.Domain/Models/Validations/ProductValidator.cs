using FluentValidation;

namespace GameStore.Domain.Models.Validations;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Product description cannot exceed 500 characters.")
            .When(p => p.Description != null);

        RuleFor(p => p.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.");

        RuleFor(p => p.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.");

        RuleFor(p => p.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.");

        RuleFor(p => p.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}
