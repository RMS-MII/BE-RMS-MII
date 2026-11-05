using FluentValidation;

namespace WEBAPI.Features.Portal.Queries.GetTest;

public sealed class GetTestValidator : AbstractValidator<GetTestQuery>
{
    public GetTestValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty();

        RuleFor(x => x.StageId)
            .NotEmpty();
    }
}
