using FluentValidation;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.Interviewers.RespondInvitation;

public class RespondInvitationValidator : AbstractValidator<RespondInvitationCommand>
{
    public RespondInvitationValidator()
    {
        RuleFor(x => x.InvitationId)
            .NotEmpty()
            .WithMessage("Interviewer assignment invitation ID is required.");

        RuleFor(x => x.Response)
            .Must(r => r == StageEmployeeStatus.Accepted || r == StageEmployeeStatus.Declined)
            .WithMessage("Response must be either 'Accepted' or 'Declined'.");

        RuleFor(x => x.Note)
            .NotEmpty()
            .When(x => x.Response == StageEmployeeStatus.Declined)
            .WithMessage("A rejection reason (Note) is required when declining an assignment.")
            .MaximumLength(500)
            .WithMessage("Note must not exceed 500 characters.");
    }
}
