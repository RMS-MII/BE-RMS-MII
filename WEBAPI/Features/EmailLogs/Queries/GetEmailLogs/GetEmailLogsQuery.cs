using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;

namespace WEBAPI.Features.EmailLogs.Queries.GetEmailLogs;

public sealed record GetEmailLogsQuery : IRequest<Result<PagedResult<EmailLogListItemDto>>>
{
    public string? Search { get; init; }
    public EmailType? Type { get; init; }
    public EmailDeliveryStatus? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
