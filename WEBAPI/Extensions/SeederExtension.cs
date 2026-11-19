using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Data.Seeders;
using WEBAPI.Entities;

namespace WEBAPI.Extensions;

public static class SeederExtension
{
    public static async Task SeedDatabaseAsync(this WebApplication app, string[]? args = null)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<RmsDbContext>>();
        var resetOnStartup = app.Configuration.GetValue<bool>("Seed:ResetToCheckpointOnStartup", false);
        var isResetRequested = resetOnStartup || (args != null && (args.Contains("--seed-reset") || args.Contains("--reset-checkpoint") || args.Contains("--reset-checkpoint-only")));
        var includeSampleBatch = app.Configuration.GetValue<bool>("Seed:IncludeSampleBatch", false);

        var performanceOptions = app.Configuration
            .GetSection(PerformanceSeedOptions.SectionName)
            .Get<PerformanceSeedOptions>() ?? new PerformanceSeedOptions();
        performanceOptions.Enabled = performanceOptions.Enabled
            || (args?.Contains("--seed-performance") == true);
        performanceOptions.Normalize();

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
            if (!await db.Database.CanConnectAsync())
            {
                if (isResetRequested)
                    throw new InvalidOperationException("Database tidak dapat dihubungi untuk reset checkpoint.");

                logger.LogWarning("Database tidak dapat dihubungi; seeder dilewati");
                return;
            }

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();
            if (isResetRequested)
            {
                logger.LogInformation("Perintah reset checkpoint seeder aktif. Me-reset database ke checkpoint seeder...");
                await DataSeeder.ResetToCheckpointAsync(
                    db, passwordHasher, includeSampleBatch, performanceOptions.ClearOnCheckpointReset);
            }
            else
            {
                await DataSeeder.SeedAsync(db, passwordHasher, includeSampleBatch);
            }

            if (args?.Contains("--seed-sample-applications") == true)
            {
                await SampleBatchApplicationsSeeder.SeedAsync(db);
            }

            if (performanceOptions.Enabled)
            {
                logger.LogInformation(
                    "[PerfSeed] Menjalankan seeder performa: kandidat={Candidates}, batch={Batches}, " +
                    "lamaran/kandidat={ApplicationsPerCandidate}, email={EmailLogs}, bulkCopy={UseSqlBulkCopy}.",
                    performanceOptions.Candidates,
                    performanceOptions.Batches,
                    performanceOptions.ApplicationsPerCandidate,
                    performanceOptions.EmailLogs,
                    performanceOptions.UseSqlBulkCopy);

                await PerformanceDataSeeder.SeedAsync(
                    db, passwordHasher, performanceOptions, logger);
            }

            // Warm up EF Core model, database connection pool, and crypto providers to eliminate first-request latency
            await WarmupServicesAsync(db, passwordHasher, logger);
        }
        catch (Exception ex)
        {
            if (isResetRequested)
                throw;

            logger.LogError(ex, "Seeder atau warmup gagal dijalankan");
        }
    }

    private static async Task WarmupServicesAsync(
        RmsDbContext db,
        IPasswordHasher<Account> passwordHasher,
        ILogger logger)
    {
        try
        {
            // 1. Force EF Core metadata model generation ahead-of-time
            _ = db.Model;

            // 2. Pre-warm connection pool with a fast query
            await db.Database.OpenConnectionAsync();
            _ = await db.Departments.AsNoTracking().AnyAsync();

            // 3. Pre-warm PBKDF2 PasswordHasher JIT & crypto provider
            var dummyAccount = new Account { Id = Guid.Empty };
            var hash = passwordHasher.HashPassword(dummyAccount, "Warmup@123!");
            _ = passwordHasher.VerifyHashedPassword(dummyAccount, hash, "Warmup@123!");

            logger.LogInformation("Database connection pool & EF Core model pre-warmed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warmup non-kritis mengalami kendala, aplikasi tetap berjalan.");
        }
    }
}
