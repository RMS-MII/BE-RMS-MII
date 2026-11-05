using FluentValidation;

namespace WEBAPI.Features.Interviewers.SubmitAssessment;

public class SubmitAssessmentValidator : AbstractValidator<SubmitAssessmentCommand>
{
    public SubmitAssessmentValidator()
    {
        RuleFor(x => x.SessionInterviewId)
            .NotEmpty()
            .WithMessage("ID sesi wawancara wajib diisi.");

        RuleFor(x => x.Score)
            .InclusiveBetween(0m, 100m)
            .WithMessage("Nilai wawancara harus berada dalam rentang 0 sampai 100.");

        RuleFor(x => x.Recommendation)
            .IsInEnum()
            .WithMessage("Rekomendasi hasil wawancara tidak valid.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Catatan evaluasi maksimal 2000 karakter.");
    }
}
