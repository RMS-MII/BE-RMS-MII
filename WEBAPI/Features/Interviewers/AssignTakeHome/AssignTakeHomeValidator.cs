using FluentValidation;

namespace WEBAPI.Features.Interviewers.AssignTakeHome;

public class AssignTakeHomeValidator : AbstractValidator<AssignTakeHomeCommand>
{
    public AssignTakeHomeValidator()
    {
        RuleFor(x => x.InterviewSessionId)
            .NotEmpty()
            .WithMessage("ID sesi wawancara wajib diisi.");

        RuleFor(x => x.EndedSubmissionAt)
            .Must(deadline => deadline > DateTime.UtcNow)
            .WithMessage("Batas waktu pengumpulan take-home test harus lebih besar dari waktu saat ini.");

        RuleFor(x => x.FileTakeHome)
            .NotEmpty().WithMessage("Tautan tugas wajib diisi.")
            .MaximumLength(200).WithMessage("Tautan tugas maksimal 200 karakter.")
            .Must(url => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
                         && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Tautan tugas harus berupa URL HTTP atau HTTPS.");

        RuleFor(x => x.TaskTitle)
            .NotEmpty()
            .WithMessage("Judul tugas wajib diisi.")
            .MaximumLength(150)
            .WithMessage("Judul tugas maksimal 150 karakter.");

        RuleFor(x => x.Instructions)
            .NotEmpty()
            .WithMessage("Instruksi tugas wajib diisi.")
            .MaximumLength(4000)
            .WithMessage("Instruksi tugas maksimal 4000 karakter.");
    }
}
