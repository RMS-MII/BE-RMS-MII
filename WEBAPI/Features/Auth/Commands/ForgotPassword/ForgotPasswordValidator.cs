using FluentValidation;

namespace WEBAPI.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(f => f.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(50);
    }
}