using FluentValidation;

namespace WEBAPI.Features.Portal.UploadSubmission;

public class UploadSubmissionValidator : AbstractValidator<UploadSubmissionCommand>
{
    public UploadSubmissionValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty().WithMessage("ID kandidat wajib diisi.");

        RuleFor(x => x.InterviewSessionId)
            .NotEmpty().WithMessage("ID sesi wawancara wajib diisi.");

        RuleFor(x => x.FileSubmission)
            .NotEmpty().WithMessage("Tautan berkas Google Drive wajib diisi.")
            .MaximumLength(200).WithMessage("Tautan berkas tidak boleh melebihi 200 karakter.")
            .Must(BeAValidUrl).WithMessage("Tautan berkas harus berupa URL yang valid (contoh: https://drive.google.com/...).");
    }

    private static bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

