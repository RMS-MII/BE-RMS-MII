using FluentValidation;

namespace WEBAPI.Features.Candidates.Queries.GetCandidates;

public sealed class GetCandidatesValidator : AbstractValidator<GetCandidatesQuery>
{
    public GetCandidatesValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page minimal bernilai 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize harus antara 1 sampai 100.");
    }
}

