using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;
using WEBAPI.Templates;

namespace WEBAPI.Features.Applications.Commands.UploadOfferingLetter;

public sealed class UploadOfferingLetterHandler(
    RmsDbContext db,
    IDateTimeProvider dateTimeProvider,
    IEmailService emailService,
    ILogger<UploadOfferingLetterHandler> logger)
    : IRequestHandler<UploadOfferingLetterCommand, Result<BatchApplicationDto>>
{
    public async Task<Result<BatchApplicationDto>> Handle(
        UploadOfferingLetterCommand request, CancellationToken ct)
    {
        var application = await db.BatchApplications
            .Include(batchApplication => batchApplication.Candidate)
            .Include(batchApplication => batchApplication.RecruitmentBatch)
            .FirstOrDefaultAsync(
                batchApplication => batchApplication.Id == request.ApplicationId, ct);

        if (application is null)
            return Result<BatchApplicationDto>.Failure(
                Error.NotFound("Application was not found."));

        if (application.RecruitmentBatch.Status == BatchStatus.Closed)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule("A closed recruitment batch cannot be edited."));

        if (application.FinalResultStatus != FinalResultStatus.Accepted ||
            application.Status != ApplicationStatus.Accepted)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule(
                    "Offering letter can only be uploaded for an accepted application."));

        if (application.OfferingLetterStatus != OfferingLetterStatus.Pending)
            return Result<BatchApplicationDto>.Failure(
                Error.BusinessRule(
                    "Offering letter has already been responded to by the candidate."));

        var now = dateTimeProvider.GetUtcNow().UtcDateTime;

        application.FileOfferingLetter = request.FileOfferingLetter.Trim();
        application.OfferingLetterStatus = OfferingLetterStatus.Pending;
        application.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        await SendOfferingLetterEmailAsync(application, ct);

        return Result<BatchApplicationDto>.Success(application.ToDto());
    }

    private async Task SendOfferingLetterEmailAsync(
        BatchApplication application, CancellationToken ct)
    {
        try
        {
            var content = EmailTemplates.OfferingLetter(
                application.Candidate.FullName,
                application.RecruitmentBatch.Name,
                application.FileOfferingLetter);

            await emailService.SendEmailAsync(
                application.Candidate.Email,
                content,
                EmailType.FinalResult,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Gagal mengirim email offering letter untuk application {ApplicationId}",
                application.Id);
        }
    }
}
