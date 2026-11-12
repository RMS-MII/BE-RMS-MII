using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;

namespace WEBAPI.Services;

public sealed class TestAttemptFinalizer(
    RmsDbContext db) : ITestAttemptFinalizer
{
    public async Task<ApplicationStageStatus> FinalizeAsync(
        ApplicationStage stage,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var answers = await db.CandidateAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationStageId == stage.Id)
            .ToListAsync(cancellationToken);

        var totalScore = answers.Sum(answer => answer.Score);
        totalScore = Math.Round(totalScore, 2, MidpointRounding.AwayFromZero);

        var application = stage.BatchApplication;

        stage.TotalScore = totalScore;
        stage.CompletedAt = now;
        stage.UpdatedAt = now;
        stage.IsAttempts = true;
        application.UpdatedAt = now;

        stage.Status = stage.BatchStage.PassingScore is decimal passingScore
            ? totalScore >= passingScore
                ? ApplicationStageStatus.Considered
                : ApplicationStageStatus.Rejected
            : ApplicationStageStatus.Submitted;

        if (stage.Status == ApplicationStageStatus.Rejected)
            application.Status = ApplicationStatus.Rejected;

        await db.SaveChangesAsync(cancellationToken);

        return stage.Status;
    }
}
