using System.Data;
using FluentValidation;

namespace WEBAPI.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(r => r.Token)
            .NotEmpty()
            .Matches("^[0-9A-Fa-f]{64}$]$");

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
            .Must(p => p.Any(c =>
                !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            .WithMessage("Password must contain symbols");
    }
}