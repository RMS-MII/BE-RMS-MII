using FluentValidation;

namespace WEBAPI.Features.Portal.Commands.FinishTest;

public sealed class FinishTestValidator : AbstractValidator<FinishTestCommand>
{
    public FinishTestValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty();

        RuleFor(x => x.StageId)
            .NotEmpty();
    }
}
