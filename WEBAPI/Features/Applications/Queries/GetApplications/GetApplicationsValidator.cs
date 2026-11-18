using FluentValidation;

namespace WEBAPI.Features.Applications.Queries.GetApplications;

public sealed class GetApplicationsValidator : AbstractValidator<GetApplicationsQuery>
{
    public GetApplicationsValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty();

        RuleFor(x => x.StageId)
            .Must(stageId => stageId is null || stageId.Value != Guid.Empty)
            .WithMessage("StageId is invalid.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(100);

        RuleFor(x => x.SortBy)
            .Must(value => value is null || new[] { "name", "code", "stage", "status", "appliedAt" }
                .Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("SortBy is invalid.");

        RuleFor(x => x.Order)
            .Must(value => value is null || new[] { "asc", "desc" }
                .Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Order is invalid.");

        RuleFor(x => x.Status)
            .Must(status => status is null || Enum.IsDefined(status.Value))
            .WithMessage("Status is invalid.");
    }
}
