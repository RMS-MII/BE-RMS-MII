using FluentValidation;

namespace WEBAPI.Features.Interviewers.SubmitAssessment;

public class SubmitAssessmentValidator : AbstractValidator<SubmitAssessmentCommand>
{
    public SubmitAssessmentValidator()
    {
        RuleFor(x => x.SessionInterviewId)
            .NotEmpty()
            .WithMessage("Interview session ID is required.");

        RuleFor(x => x.Score)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Interview score must be between 0 and 100.");

        RuleFor(x => x.Recommendation)
            .IsInEnum()
            .WithMessage("Interview result recommendation is invalid.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Evaluation note must not exceed 2000 characters.");
    }
}
