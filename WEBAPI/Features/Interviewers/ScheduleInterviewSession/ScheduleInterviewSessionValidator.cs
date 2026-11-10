using FluentValidation;

namespace WEBAPI.Features.Interviewers.ScheduleInterviewSession;

public class ScheduleInterviewSessionValidator : AbstractValidator<ScheduleInterviewSessionCommand>
{
    public ScheduleInterviewSessionValidator()
    {
        RuleFor(x => x.ApplicationStageId)
            .NotEmpty()
            .WithMessage("ID ApplicationStage wajib diisi.");

        RuleFor(x => x.SessionName)
            .NotEmpty()
            .WithMessage("Nama sesi wawancara wajib diisi.")
            .MaximumLength(200)
            .WithMessage("Nama sesi wawancara maksimal 200 karakter.");

        RuleFor(x => x.ScheduledStart)
            .NotEmpty()
            .WithMessage("Waktu mulai sesi wawancara wajib diisi.");

        RuleFor(x => x.ScheduledEnd)
            .NotEmpty()
            .WithMessage("Waktu selesai sesi wawancara wajib diisi.")
            .GreaterThan(x => x.ScheduledStart)
            .WithMessage("Waktu selesai harus lebih besar dari waktu mulai.");

        When(x => x.IsOnline, () =>
        {
            RuleFor(x => x.MeetingUrl)
                .NotEmpty()
                .WithMessage("Tautan meeting (MeetingUrl) wajib diisi untuk wawancara daring (online).")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Format tautan meeting (MeetingUrl) tidak valid.");
        });

        When(x => !x.IsOnline, () =>
        {
            RuleFor(x => x.Location)
                .NotEmpty()
                .WithMessage("Lokasi ruangan/kantor wajib diisi untuk wawancara tatap muka (offline).")
                .MaximumLength(255)
                .WithMessage("Lokasi maksimal 255 karakter.");
        });

        RuleFor(x => x.InterviewerEmployeeIds)
            .NotEmpty()
            .WithMessage("Minimal harus memilih satu pewawancara untuk sesi wawancara.");

        RuleFor(x => x.LeadEmployeeId)
            .Must((command, leadEmployeeId) =>
                !leadEmployeeId.HasValue ||
                command.InterviewerEmployeeIds.Contains(leadEmployeeId.Value))
            .WithMessage("Lead interviewer harus termasuk dalam daftar pewawancara yang dipilih.");
    }
}
