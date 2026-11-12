using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WEBAPI.Common;
using WEBAPI.Data;
using WEBAPI.Entities;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Portal;
using WEBAPI.Services;

namespace WEBAPI.Services;

/// <summary>
/// Background service yang mengecek secara berkala attempt tes yang sedang berjalan
/// dan melewati batas waktu pengerjaan. Saat batas waktu habis, sesi ditutup
/// (CompletedAt) dan jawaban yang tersimpan dihitung skornya.
/// </summary>
public sealed class TestAttemptTimeoutBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<TestAttemptTimeoutBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
                var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
                var finalizer = scope.ServiceProvider.GetRequiredService<ITestAttemptFinalizer>();

                var now = dateTimeProvider.GetUtcNow().UtcDateTime;

                var activeStages = await db.ApplicationStages
                    .Include(applicationStage => applicationStage.BatchStage)
                    .Include(applicationStage => applicationStage.BatchApplication)
                    .Where(applicationStage =>
                        applicationStage.Status == ApplicationStageStatus.InProgress &&
                        applicationStage.AttemptStartedAt != null)
                    .ToListAsync(stoppingToken);

                foreach (var stage in activeStages)
                {
                    if (!CandidateActionPolicy.IsActive(stage))
                        continue;

                    if (TestAttemptPolicy.IsPastDeadline(stage, now))
                    {
                        stage.Status = ApplicationStageStatus.Expired;
                        stage.CompletedAt = now;
                        stage.UpdatedAt = now;
                        stage.BatchApplication.UpdatedAt = now;

                        await db.SaveChangesAsync(stoppingToken);

                        // Batas waktu habis: tutup sesi lalu hitung skor dari jawaban
                        // yang berhasil tersimpan sebelum waktu habis.
                        await finalizer.FinalizeAsync(stage, now, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "An error occurred while processing expired test attempts.");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }
}
