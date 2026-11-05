using FluentValidation;

namespace WEBAPI.Features.Applications.Queries.GetApplicationRecap;

public sealed class GetApplicationRecapValidator : AbstractValidator<GetApplicationRecapQuery>
{
    public GetApplicationRecapValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
