using FluentValidation;

namespace WEBAPI.Features.Portal.Queries.GetCandidateTasks;

public sealed class GetCandidateTasksValidator : AbstractValidator<GetCandidateTasksQuery>
{
    public GetCandidateTasksValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty();
    }
}
