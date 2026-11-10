using FluentValidation;

namespace WEBAPI.Features.Applications.Commands.BulkStartNextStage;

public sealed class BulkStartNextStageValidator : AbstractValidator<BulkStartNextStageCommand>
{
    public BulkStartNextStageValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty()
            .WithMessage("ID batch wajib diisi.");

        RuleFor(x => x.StageId)
            .NotEmpty()
            .WithMessage("ID tahap wajib diisi.");
    }
}
