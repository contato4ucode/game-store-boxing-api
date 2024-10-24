using FluentValidation;
using FluentValidation.Results;
using GameStore.Domain.Interfaces.UoW;

namespace GameStore.Domain.Models.Validations;

public class BoxValidator : AbstractValidator<Box>
{
    private readonly IUnitOfWork _unitOfWork;

    public BoxValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void ConfigureRulesForCreate()
    {
        RuleFor(b => b.Name)
            .NotEmpty().WithMessage("Box name cannot be empty.")
            .MaximumLength(100).WithMessage("Box name cannot exceed 100 characters.")
            .MustAsync(async (name, cancellation) =>
            {
                var existingBox = await _unitOfWork.Boxes.Find(b => b.Name == name);
                return !existingBox.Any();
            }).WithMessage("A box with this name already exists.");

        ConfigureCommonRules();
    }

    public void ConfigureRulesForUpdate(Box existingBox)
    {
        RuleFor(b => b.Name)
            .NotEmpty().WithMessage("Box name cannot be empty.")
            .MaximumLength(100).WithMessage("Box name cannot exceed 100 characters.")
            .MustAsync(async (box, name, cancellation) =>
            {
                var existingBoxes = await _unitOfWork.Boxes.Find(b => b.Name == name);
                return !existingBoxes.Any() || existingBoxes.First().Id == box.Id;
            }).WithMessage("A box with this name already exists.")
            .When(b => existingBox.Name != b.Name);

        ConfigureCommonRules();
    }

    public void ConfigureCommonRules()
    {
        RuleFor(b => b.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.");

        RuleFor(b => b.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.");

        RuleFor(b => b.Length)
            .GreaterThan(0).WithMessage("Length must be greater than 0.");
    }

    public async Task<ValidationResult> ValidateVolumeAsync(Box box)
    {
        var volumeValidator = new InlineValidator<Box>();
        volumeValidator.RuleFor(b => b.Volume)
            .GreaterThan(0).WithMessage("The box volume must be greater than 0.");

        return await volumeValidator.ValidateAsync(box);
    }
}
