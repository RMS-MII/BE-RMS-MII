using FluentValidation;

namespace WEBAPI.Features.Batches.Queries.GetBatches;

public sealed class GetBatchesValidator : AbstractValidator<GetBatchesQuery>
{
    public GetBatchesValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MaximumLength(50);

        RuleFor(x => x.Status)
            .Must(status => status is null || Enum.IsDefined(status.Value))
            .WithMessage("Status is invalid.");

        RuleFor(x => x.DepartmentId)
            .Must(departmentId => departmentId is null || departmentId != Guid.Empty)
            .WithMessage("Department id is invalid.");
    }
}
