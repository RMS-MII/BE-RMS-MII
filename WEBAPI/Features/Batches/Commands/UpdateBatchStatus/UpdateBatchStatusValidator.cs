using FluentValidation;

namespace WEBAPI.Features.Batches.Commands.UpdateBatchStatus;

public sealed class UpdateBatchStatusValidator : AbstractValidator<UpdateBatchStatusCommand>
{
    public UpdateBatchStatusValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("A valid batch status is required.");
    }
}
