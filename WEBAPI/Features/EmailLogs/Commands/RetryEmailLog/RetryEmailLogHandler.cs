using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Features.EmailLogs.Commands.RetryEmailLog;

public sealed class RetryEmailLogHandler(RmsDbContext db)
    : IRequestHandler<RetryEmailLogCommand, Result<RetryEmailLogResponse>>
{
    public async Task<Result<RetryEmailLogResponse>> Handle(RetryEmailLogCommand request, CancellationToken ct)
    {
        var updated = await db.EmailLogs
            .Where(log => log.Id == request.Id && log.Status == EmailDeliveryStatus.Failed)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(log => log.Status, EmailDeliveryStatus.Pending)
                .SetProperty(log => log.AttemptedAt, DateTime.UtcNow)
                .SetProperty(log => log.SentAt, (DateTime?)null)
                .SetProperty(log => log.ErrorMessage, (string?)null), ct);

        if (updated == 1)
            return Result<RetryEmailLogResponse>.Success(new(request.Id, EmailDeliveryStatus.Pending));

        var exists = await db.EmailLogs.AnyAsync(log => log.Id == request.Id, ct);
        return Result<RetryEmailLogResponse>.Failure(exists
            ? Error.Conflict("Only failed email attempts can be retried.")
            : Error.NotFound("Email log was not found."));
    }
}
