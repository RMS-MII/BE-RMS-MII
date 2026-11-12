using FluentValidation;

namespace WEBAPI.Features.Applications.Queries.GetTestResult;

public sealed class GetTestResultValidator : AbstractValidator<GetTestResultQuery>
{
    public GetTestResultValidator()
    {
        RuleFor(x => x.ApplicationStageId)
            .NotEmpty();
    }
}
