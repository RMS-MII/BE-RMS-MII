using FluentValidation;

namespace WEBAPI.Features.EmailLogs.Queries.GetEmailLogs;

public sealed class GetEmailLogsValidator : AbstractValidator<GetEmailLogsQuery>
{
    public GetEmailLogsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.Type)
            .Must(type => type is null || Enum.IsDefined(type.Value));
        RuleFor(x => x.Status)
            .Must(status => status is null || Enum.IsDefined(status.Value));
        RuleFor(x => x.From)
            .Must((query, from) => from is null || query.To is null || from <= query.To)
            .WithMessage("The start date must be before or equal to the end date.");
    }
}
