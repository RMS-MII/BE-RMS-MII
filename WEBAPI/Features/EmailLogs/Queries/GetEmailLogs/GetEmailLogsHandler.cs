using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.EmailLogs.Queries.GetEmailLogs;

public sealed class GetEmailLogsHandler(RmsDbContext db)
    : IRequestHandler<GetEmailLogsQuery, Result<PagedResult<EmailLogListItemDto>>>
{
    public async Task<Result<PagedResult<EmailLogListItemDto>>> Handle(
        GetEmailLogsQuery request,
        CancellationToken ct)
    {
        var query = db.EmailLogs.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(log => log.Status == request.Status.Value);

        if (request.Type.HasValue)
            query = query.Where(log => log.Type == request.Type.Value);

        if (request.From.HasValue)
            query = query.Where(log => log.AttemptedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(log => log.AttemptedAt <= request.To.Value);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(log =>
                log.RecipientEmail.Contains(search) ||
                log.Subject.Contains(search));
        }

        var totalItems = await query.CountAsync(ct);
        var logs = await query
            .OrderByDescending(log => log.AttemptedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(log => new EmailLogListItemDto
            {
                Id = log.Id,
                SenderEmail = log.SenderEmail,
                RecipientEmail = log.RecipientEmail,
                Type = log.Type,
                Subject = log.Subject,
                Status = log.Status,
                AttemptedAt = log.AttemptedAt,
                SentAt = log.SentAt,
                ErrorMessage = log.ErrorMessage,
                CanRetry = log.Status == WEBAPI.Entities.Enums.EmailDeliveryStatus.Failed
            })
            .ToListAsync(ct);

        return Result<PagedResult<EmailLogListItemDto>>.Success(
            new PagedResult<EmailLogListItemDto>(
                logs,
                request.Page,
                request.PageSize,
                totalItems));
    }
}
