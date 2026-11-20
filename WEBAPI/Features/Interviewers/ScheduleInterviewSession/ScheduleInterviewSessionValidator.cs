using FluentValidation;

namespace WEBAPI.Features.Interviewers.ScheduleInterviewSession;

public class ScheduleInterviewSessionValidator : AbstractValidator<ScheduleInterviewSessionCommand>
{
    public ScheduleInterviewSessionValidator()
    {
        RuleFor(x => x.ApplicationStageId)
            .NotEmpty()
            .WithMessage("ApplicationStage ID is required.");

        RuleFor(x => x.SessionName)
            .NotEmpty()
            .WithMessage("Interview session name is required.")
            .MaximumLength(200)
            .WithMessage("Interview session name must not exceed 200 characters.");

        RuleFor(x => x.ScheduledStart)
            .NotEmpty()
            .WithMessage("Interview session start time is required.");

        RuleFor(x => x.ScheduledEnd)
            .NotEmpty()
            .WithMessage("Interview session end time is required.")
            .GreaterThan(x => x.ScheduledStart)
            .WithMessage("End time must be after start time.");

        When(x => x.IsOnline, () =>
        {
            RuleFor(x => x.MeetingUrl)
                .NotEmpty()
                .WithMessage("A meeting link (MeetingUrl) is required for an online interview.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Meeting link (MeetingUrl) format is invalid.");
        });

        When(x => !x.IsOnline, () =>
        {
            RuleFor(x => x.Location)
                .NotEmpty()
                .WithMessage("A room/office location is required for an onsite (offline) interview.")
                .MaximumLength(255)
                .WithMessage("Location must not exceed 255 characters.");
        });

        RuleFor(x => x.InterviewerEmployeeIds)
            .NotEmpty()
            .WithMessage("At least one interviewer must be selected for the interview session.");

        RuleFor(x => x.LeadEmployeeId)
            .Must((command, leadEmployeeId) =>
                !leadEmployeeId.HasValue ||
                command.InterviewerEmployeeIds.Contains(leadEmployeeId.Value))
            .WithMessage("The lead interviewer must be included in the selected interviewers.");
    }
}
