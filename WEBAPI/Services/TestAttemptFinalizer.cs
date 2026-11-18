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
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var updated = await db.ApplicationStages
            .Where(item => item.Id == stage.Id && !item.IsAttempts && item.AttemptStartedAt != null &&
                (item.Status == ApplicationStageStatus.InProgress || item.Status == ApplicationStageStatus.Expired))
            .ExecuteUpdateAsync(update => update.SetProperty(item => item.UpdatedAt, now), cancellationToken);
        if (updated == 0)
        {
            await db.Entry(stage).ReloadAsync(cancellationToken);
            return stage.Status;
        }

        var answers = await db.CandidateAnswers
            .AsNoTracking()
            .Where(answer => answer.ApplicationStageId == stage.Id)
            .ToListAsync(cancellationToken);

        var rawScore = answers.Sum(answer => answer.Score);

        // Skor maksimum dihitung dari seluruh soal aktif tes agar skor akhir dapat
        // dinormalisasi ke skala 0-100, termasuk ketika jawaban hanya tersimpan sebagian.
        var departmentId = await db.RecruitmentBatches
            .AsNoTracking()
            .Where(batch => batch.Id == stage.BatchApplication.RecruitmentBatchId)
            .Select(batch => batch.DepartmentId)
            .FirstOrDefaultAsync(cancellationToken);

        var questions = await db.Questions
            .AsNoTracking()
            .Where(question =>
                question.IsActive &&
                question.QuestionBank.DepartmentId == departmentId &&
                question.QuestionBank.StageTypeId == stage.BatchStage.StageTypeId)
            .Include(question => question.QuestionOptions)
            .ToListAsync(cancellationToken);

        var maxRawScore = TestScoreCalculator.CalculateMaxRawScore(questions);
        var totalScore = TestScoreCalculator.Normalize(rawScore, maxRawScore);

        var application = stage.BatchApplication;

        stage.TotalScore = totalScore;
        stage.CompletedAt = now;
        stage.UpdatedAt = now;
        stage.IsAttempts = true;
        application.UpdatedAt = now;

        // Standalone applications are evaluated by HR after the test is submitted.
        // The score is stored now, but the passing threshold must not publish a decision.
        stage.Status = application.IsStandalone
            ? ApplicationStageStatus.Submitted
            : stage.BatchStage.PassingScore is decimal passingScore
                ? totalScore >= passingScore
                    ? ApplicationStageStatus.Considered
                    : ApplicationStageStatus.Rejected
                : ApplicationStageStatus.Submitted;

        if (stage.Status == ApplicationStageStatus.Rejected)
            application.Status = ApplicationStatus.Rejected;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return stage.Status;
    }
}
