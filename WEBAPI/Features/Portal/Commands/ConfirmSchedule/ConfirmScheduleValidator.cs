using FluentValidation;

namespace WEBAPI.Features.Portal.Commands.ConfirmSchedule;

public sealed class ConfirmScheduleValidator : AbstractValidator<ConfirmScheduleCommand>
{
    public ConfirmScheduleValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();
    }
}
