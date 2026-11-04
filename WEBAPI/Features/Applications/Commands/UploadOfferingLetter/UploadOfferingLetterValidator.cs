using FluentValidation;

namespace WEBAPI.Features.Applications.Commands.UploadOfferingLetter;

public sealed class UploadOfferingLetterValidator : AbstractValidator<UploadOfferingLetterCommand>
{
    public UploadOfferingLetterValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.FileOfferingLetter)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeHttpsUrl)
            .WithMessage("Offering letter link must be a valid absolute HTTPS URL.");
    }

    private static bool BeHttpsUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
               && uri.Scheme == Uri.UriSchemeHttps
               && !string.IsNullOrWhiteSpace(uri.Host);
    }
}
