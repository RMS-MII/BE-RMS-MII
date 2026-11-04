using FluentValidation;

namespace WEBAPI.Features.Applications.Commands.StartApplicationStage;

public sealed class StartApplicationStageValidator : AbstractValidator<StartApplicationStageCommand>
{
    public StartApplicationStageValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();
    }
}
