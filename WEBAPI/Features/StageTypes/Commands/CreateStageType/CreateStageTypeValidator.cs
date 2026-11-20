using FluentValidation;

namespace WEBAPI.Features.StageTypes.Commands.CreateStageType;

public sealed class CreateStageTypeValidator : AbstractValidator<CreateStageTypeCommand>
{
    public CreateStageTypeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Stage type name is required.")
            .MaximumLength(30).WithMessage("Stage type name must not exceed 30 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(100).WithMessage("Description must not exceed 100 characters.");

        RuleFor(x => x.StageCategory)
            .IsInEnum().WithMessage("Stage category is invalid.");
    }
}

