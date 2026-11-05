using FluentValidation;

namespace WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;

public class RequestRescheduleValidator : AbstractValidator<RequestRescheduleCommand>
{
    public RequestRescheduleValidator()
    {
        RuleFor(x => x.InterviewSessionId)
            .NotEmpty().WithMessage("ID sesi wawancara wajib diisi.");

        RuleFor(x => x.ProposedStart)
            .NotEmpty().WithMessage("Waktu mulai yang diusulkan wajib diisi.")
            .Must(start => start > DateTime.UtcNow)
            .WithMessage("Waktu mulai yang diusulkan harus di masa mendatang.");

        RuleFor(x => x.ProposedEnd)
            .NotEmpty().WithMessage("Waktu selesai yang diusulkan wajib diisi.")
            .GreaterThan(x => x.ProposedStart)
            .WithMessage("Waktu selesai harus lebih besar dari waktu mulai.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Alasan pengajuan reschedule wajib diisi.")
            .MaximumLength(100).WithMessage("Alasan tidak boleh melebihi 100 karakter.");
    }
}

