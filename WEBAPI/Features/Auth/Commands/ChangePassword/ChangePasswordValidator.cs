using FluentValidation;

namespace WEBAPI.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Must(p => p.Any(char.IsUpper))
                .WithMessage("Password must contain an uppercase letter")
            .Must(p => p.Any(char.IsLower))
                .WithMessage("Password must contain a lowercase")
            .Must(p => p.Any(char.IsDigit))
                .WithMessage("Password must contain a number")
            .Must(p => p.Any(c=> 
                !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
                .WithMessage("Password must contain symbols")
            .Must((command, newPassword) =>
                newPassword != command.CurrentPassword)
            .WithMessage("New password must be different than current password.");
    }
}