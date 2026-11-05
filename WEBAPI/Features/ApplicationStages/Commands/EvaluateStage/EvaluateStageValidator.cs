using FluentValidation;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.ApplicationStages.Commands.EvaluateStage;

public sealed class EvaluateStageValidator : AbstractValidator<EvaluateStageCommand>
{
    public EvaluateStageValidator()
    {
        RuleFor(x => x.StageId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => status is ApplicationStageStatus.Prioritized
                or ApplicationStageStatus.Considered
                or ApplicationStageStatus.Rejected)
            .WithMessage("Status must be Prioritized, Considered, or Rejected.");

        RuleFor(x => x.Notes)
            .NotEmpty()
            .MaximumLength(500);
    }
}
