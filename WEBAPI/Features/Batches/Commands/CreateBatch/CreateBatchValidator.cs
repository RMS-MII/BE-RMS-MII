using FluentValidation;

namespace WEBAPI.Features.Batches.Commands.CreateBatch;

public sealed class CreateBatchValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId is required.");

        When(x => !string.IsNullOrWhiteSpace(x.BatchCode), () =>
        {
            RuleFor(x => x.BatchCode)
                .MaximumLength(50).WithMessage("BatchCode must not exceed 50 characters.");
        });

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}
