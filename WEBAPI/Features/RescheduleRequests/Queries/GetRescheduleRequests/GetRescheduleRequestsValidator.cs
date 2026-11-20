using FluentValidation;

namespace WEBAPI.Features.RescheduleRequests.Queries.GetRescheduleRequests;

public class GetRescheduleRequestsValidator : AbstractValidator<GetRescheduleRequestsQuery>
{
    public GetRescheduleRequestsValidator()
    {
        RuleFor(x => x.SessionId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("Interview session ID is invalid.");
    }
}

