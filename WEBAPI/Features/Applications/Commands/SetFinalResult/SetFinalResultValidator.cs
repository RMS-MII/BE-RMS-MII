using FluentValidation;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.Applications.Commands.SetFinalResult;

public sealed class SetFinalResultValidator : AbstractValidator<SetFinalResultCommand>
{
    public SetFinalResultValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.FinalResultStatus)
            .IsInEnum()
            .Must(status => status is FinalResultStatus.Accepted
                or FinalResultStatus.Rejected)
            .WithMessage("Final result status must be Accepted or Rejected.");

        RuleFor(x => x.Notes)
            .NotEmpty()
            .MaximumLength(500);
    }
}
