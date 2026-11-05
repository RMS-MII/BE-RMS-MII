using FluentValidation;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.RescheduleRequests.Commands.ReviewReschedule;

public class ReviewRescheduleValidator : AbstractValidator<ReviewRescheduleCommand>
{
    public ReviewRescheduleValidator()
    {
        RuleFor(x => x.RescheduleRequestId)
            .NotEmpty().WithMessage("ID permohonan reschedule wajib diisi.");

        RuleFor(x => x.Decision)
            .Must(d => d is RescheduleStatus.Approved or RescheduleStatus.Rejected)
            .WithMessage("Keputusan tinjauan hanya boleh 'Approved' atau 'Rejected'.");

        RuleFor(x => x.ReviewNotes)
            .MaximumLength(100).WithMessage("Catatan tinjauan tidak boleh melebihi 100 karakter.");
    }
}

