using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.EmailLogs.Commands.RetryEmailLog;

public sealed record RetryEmailLogCommand(Guid Id) : IRequest<Result<RetryEmailLogResponse>>;

public sealed record RetryEmailLogResponse(Guid Id, EmailDeliveryStatus Status);
