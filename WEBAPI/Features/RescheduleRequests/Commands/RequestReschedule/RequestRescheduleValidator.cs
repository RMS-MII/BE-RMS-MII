using FluentValidation;

namespace WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;

public class RequestRescheduleValidator : AbstractValidator<RequestRescheduleCommand>
{
    private static readonly TimeZoneInfo Wib = TimeZoneInfo.CreateCustomTimeZone(
        "WIB", TimeSpan.FromHours(7), "WIB", "WIB");

    public RequestRescheduleValidator()
    {
        RuleFor(x => x.InterviewSessionId)
            .NotEmpty().WithMessage("Interview session ID is required.");

        RuleFor(x => x.ProposedStart)
            .NotEmpty().WithMessage("Proposed start time is required.")
            .Must((request, _) => request.ProposedStartUtc > DateTime.UtcNow)
            .WithMessage("Proposed start time must be in the future.");

        RuleFor(x => x.ProposedEnd)
            .NotEmpty().WithMessage("Proposed end time is required.")
            .Must((request, _) => request.ProposedEndUtc > request.ProposedStartUtc)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x)
            .Must(x => ToWibDate(x.ProposedStartUtc) == ToWibDate(x.ProposedEndUtc))
            .WithMessage("Start and end times must be on the same WIB date.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A reschedule reason is required.")
            .MaximumLength(100).WithMessage("Reason must not exceed 100 characters.");
    }

    private static DateOnly ToWibDate(DateTime utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, Wib));
}

