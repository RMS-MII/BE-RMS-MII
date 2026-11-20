using FluentValidation;

namespace WEBAPI.Features.Interviewers.AssignTakeHome;

public class AssignTakeHomeValidator : AbstractValidator<AssignTakeHomeCommand>
{
    public AssignTakeHomeValidator()
    {
        RuleFor(x => x.InterviewSessionId)
            .NotEmpty()
            .WithMessage("Interview session ID is required.");

        RuleFor(x => x.EndedSubmissionAt)
            .Must(deadline => deadline > DateTime.UtcNow)
            .WithMessage("The take-home test submission deadline must be in the future.");

        RuleFor(x => x.FileTakeHome)
            .NotEmpty().WithMessage("Task link is required.")
            .MaximumLength(200).WithMessage("Task link must not exceed 200 characters.")
            .Must(url => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Task link must be an HTTP or HTTPS URL.");

        RuleFor(x => x.TaskTitle)
            .NotEmpty()
            .WithMessage("Task title is required.")
            .MaximumLength(150)
            .WithMessage("Task title must not exceed 150 characters.");

        RuleFor(x => x.Instructions)
            .NotEmpty()
            .WithMessage("Task instructions are required.")
            .MaximumLength(4000)
            .WithMessage("Task instructions must not exceed 4000 characters.");
    }
}
