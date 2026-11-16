using FluentValidation;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Commands.RegisterCandidates;

public sealed class RegisterCandidatesValidator : AbstractValidator<RegisterCandidatesCommand>
{
    private const int MaxCandidates = 100;

    public RegisterCandidatesValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty();

        When(x => !x.IsPublicApplication, () =>
        {
            RuleFor(x => x.Candidates)
                .NotEmpty()
                .Must(candidates => candidates is { Count: <= MaxCandidates })
                .WithMessage($"A batch can register at most {MaxCandidates} candidates per request.")
                .Must(HaveUniqueEmails)
                .WithMessage("Candidate emails must be unique within the request.")
                .Must(HaveUniquePhoneNumbers)
                .WithMessage("Candidate phone numbers must be unique within the request.");

            RuleForEach(x => x.Candidates).SetValidator(new RegisterCandidateValidator());
        });

        When(x => x.IsPublicApplication, () =>
        {
            RuleFor(x => x.Candidates).Empty();
            RuleFor(x => x.ExpectedSalary).InclusiveBetween(0m, 922337203685477.5807m);
        });
    }

    internal static bool IsOptionalWebUrl(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) &&
         uri.Scheme is "http" or "https" && uri.Host.Length > 0);

    private static bool HaveUniqueEmails(IReadOnlyList<CreateCandidateDto>? candidates)
    {
        if (candidates is null)
            return false;

        var emails = candidates
            .Select(candidate => candidate?.Email?.Trim() ?? string.Empty)
            .Where(email => email.Length > 0)
            .ToList();

        return emails.Distinct(StringComparer.OrdinalIgnoreCase).Count() == emails.Count;
    }

    private static bool HaveUniquePhoneNumbers(IReadOnlyList<CreateCandidateDto>? candidates)
    {
        if (candidates is null)
            return false;

        var phoneNumbers = candidates
            .Select(candidate => candidate?.PhoneNumber?.Trim() ?? string.Empty)
            .Where(phoneNumber => phoneNumber.Length > 0)
            .ToList();

        return phoneNumbers.Distinct(StringComparer.OrdinalIgnoreCase).Count() == phoneNumbers.Count;
    }
}

public sealed class RegisterCandidateValidator : AbstractValidator<CreateCandidateDto>
{
    public RegisterCandidateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Gender)
            .Must(gender => Enum.IsDefined(gender))
            .WithMessage("Gender is invalid.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^\+?[0-9][0-9\s\-()]{6,19}$")
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.BirthDate)
            .Must(birthDate => birthDate != default)
            .WithMessage("Birth date is required.")
            .Must(birthDate => birthDate < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Birth date must be in the past.");

        RuleFor(x => x.Education)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Institution)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ExpectedSalary)
            .InclusiveBetween(0m, 922337203685477.5807m);

        RuleFor(x => x.Gpa)
            .InclusiveBetween(0m, 4m);

        RuleFor(x => x.Cv)
            .MaximumLength(200);

        RuleFor(x => x.Portfolio)
            .MaximumLength(200);
    }
}
