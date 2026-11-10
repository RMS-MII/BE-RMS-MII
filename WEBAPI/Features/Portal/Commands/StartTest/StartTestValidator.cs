using FluentValidation;

namespace WEBAPI.Features.Portal.Commands.StartTest;

public sealed class StartTestValidator : AbstractValidator<StartTestCommand>
{
    public StartTestValidator()
    {
        RuleFor(command => command.CandidateId).NotEmpty();
        RuleFor(command => command.StageId).NotEmpty();
    }
}
