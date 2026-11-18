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

        RuleFor(x => x.Gender)
            .Must(value => value is null || Enum.IsDefined(value.Value))
            .WithMessage("Gender is invalid.");

        RuleFor(x => x.BatchStatus)
            .Must(value => value is null || new[] { "applied", "pool" }
                .Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("BatchStatus is invalid.");

        RuleFor(x => x.SortBy)
            .Must(value => value is null || new[] { "name", "education", "gpa", "salary", "batches", "createdAt" }
                .Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SortBy is invalid.");

        RuleFor(x => x.Order)
            .Must(value => value is null || new[] { "asc", "desc" }
                .Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Order is invalid.");
    }
}

