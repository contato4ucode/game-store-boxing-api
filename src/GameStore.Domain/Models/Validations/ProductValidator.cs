using FluentValidation;
using GameStore.Domain.Interfaces.UoW;

namespace GameStore.Domain.Models.Validations;

public class ProductValidator : AbstractValidator<Product>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void ConfigureRulesForCreate()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.")
            .MustAsync(async (name, cancellation) =>
            {
                var existingProduct = await _unitOfWork.Products.Find(p => p.Name == name);
                return !existingProduct.Any();
            }).WithMessage("A product with this name already exists.");

        ConfigureCommonRules();
    }

    public void ConfigureRulesForUpdate(Product existingProduct)
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.")
            .MustAsync(async (product, name, cancellation) =>
            {
                var existingProducts = await _unitOfWork.Products.Find(p => p.Name == name);
                return !existingProducts.Any() || existingProducts.First().Id == product.Id;
            }).WithMessage("A product with this name already exists.")
            .When(p => existingProduct.Name != p.Name);

        ConfigureCommonRules();
    }

    private void ConfigureCommonRules()
    {
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
