using FluentValidation;

namespace WEBAPI.Features.Applications.Queries.GetApplications;

public sealed class GetApplicationsValidator : AbstractValidator<GetApplicationsQuery>
{
    public GetApplicationsValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty();

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .Must(status => status is null || Enum.IsDefined(status.Value))
            .WithMessage("Status is invalid.");
    }
}
