using FluentValidation;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.RescheduleRequests.Commands.ReviewReschedule;

public class ReviewRescheduleValidator : AbstractValidator<ReviewRescheduleCommand>
{
    public ReviewRescheduleValidator()
    {
        RuleFor(x => x.RescheduleRequestId)
            .NotEmpty().WithMessage("Reschedule request ID is required.");

        RuleFor(x => x.Decision)
            .Must(d => d is RescheduleStatus.Approved or RescheduleStatus.Rejected)
            .WithMessage("Review decision must be either 'Approved' or 'Rejected'.");

        RuleFor(x => x.ReviewNotes)
            .MaximumLength(100).WithMessage("Review note must not exceed 100 characters.");
    }
}

