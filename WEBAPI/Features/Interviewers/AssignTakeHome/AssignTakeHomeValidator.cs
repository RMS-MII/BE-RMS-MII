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
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Batas waktu pengumpulan take-home test harus lebih besar dari waktu saat ini.");

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
