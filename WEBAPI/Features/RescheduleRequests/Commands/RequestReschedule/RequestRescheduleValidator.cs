using FluentValidation;

namespace WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;

public class RequestRescheduleValidator : AbstractValidator<RequestRescheduleCommand>
{
    private static readonly TimeZoneInfo Wib = TimeZoneInfo.CreateCustomTimeZone(
        "WIB", TimeSpan.FromHours(7), "WIB", "WIB");

    public RequestRescheduleValidator()
    {
        RuleFor(x => x.InterviewSessionId)
            .NotEmpty().WithMessage("ID sesi wawancara wajib diisi.");

        RuleFor(x => x.ProposedStart)
            .NotEmpty().WithMessage("Waktu mulai yang diusulkan wajib diisi.")
            .Must((request, _) => request.ProposedStartUtc > DateTime.UtcNow)
            .WithMessage("Waktu mulai yang diusulkan harus di masa mendatang.");

        RuleFor(x => x.ProposedEnd)
            .NotEmpty().WithMessage("Waktu selesai yang diusulkan wajib diisi.")
            .Must((request, _) => request.ProposedEndUtc > request.ProposedStartUtc)
            .WithMessage("Waktu selesai harus lebih besar dari waktu mulai.");

        RuleFor(x => x)
            .Must(x => ToWibDate(x.ProposedStartUtc) == ToWibDate(x.ProposedEndUtc))
            .WithMessage("Waktu mulai dan selesai harus berada pada tanggal WIB yang sama.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Alasan pengajuan reschedule wajib diisi.")
            .MaximumLength(100).WithMessage("Alasan tidak boleh melebihi 100 karakter.");
    }

    private static DateOnly ToWibDate(DateTime utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, Wib));
}

