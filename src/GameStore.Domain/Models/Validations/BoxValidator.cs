using FluentValidation;

namespace GameStore.Domain.Models.Validations;

public class BoxValidator : AbstractValidator<Box>
{
    public BoxValidator()
    {
        RuleFor(b => b.Name)
            .NotEmpty().WithMessage("Box name cannot be empty.")
            .MaximumLength(100).WithMessage("Box name cannot exceed 100 characters.");

        RuleFor(b => b.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.");

        RuleFor(b => b.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.");

        RuleFor(b => b.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.");
    }
}
