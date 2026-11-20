using FluentValidation;

namespace WEBAPI.Features.Interviewers.ConfirmSession;

public class ConfirmSessionValidator : AbstractValidator<ConfirmSessionCommand>
{
    public ConfirmSessionValidator()
    {
        RuleFor(x => x.SessionInterviewId)
            .NotEmpty()
            .WithMessage("Interview session ID is required.");
    }
}
