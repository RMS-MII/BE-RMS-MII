using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Models;
using WEBAPI.Services;

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
            var subject = $"Surat Penawaran - {application.RecruitmentBatch.Name}";

            var body =
                $"""
                Halo {application.Candidate.FullName},

                Selamat! Anda diterima pada batch {application.RecruitmentBatch.Name}.

                Surat penawaran dapat diakses melalui tautan berikut:
                {application.FileOfferingLetter}

                Silakan tinjau surat penawaran dan berikan respons sesuai instruksi.

                Terima kasih.
                """;

            await emailService.SendEmailAsync(
                application.Candidate.Email,
                subject,
                body,
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
