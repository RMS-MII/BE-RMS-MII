using FluentValidation;

namespace WEBAPI.Features.Portal.UploadSubmission;

public class UploadSubmissionValidator : AbstractValidator<UploadSubmissionCommand>
{
    public UploadSubmissionValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty().WithMessage("Candidate ID is required.");

        RuleFor(x => x.InterviewSessionId)
            .NotEmpty().WithMessage("Interview session ID is required.");

        RuleFor(x => x.FileSubmission)
            .NotEmpty().WithMessage("Google Drive file link is required.")
            .MaximumLength(200).WithMessage("File link must not exceed 200 characters.")
            .Must(BeAValidUrl).WithMessage("File link must be a valid URL (e.g. https://drive.google.com/...).");
    }

    private static bool BeAValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

