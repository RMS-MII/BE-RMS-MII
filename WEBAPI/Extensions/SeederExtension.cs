using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Data.Seeders;
using WEBAPI.Entities;

namespace WEBAPI.Extensions;

public static class SeederExtension
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<RmsDbContext>>();

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
            if (!await db.Database.CanConnectAsync())
            {
                logger.LogWarning("Database tidak dapat dihubungi; seeder dilewati");
                return;
            }

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();
            var resetOnStartup = app.Configuration.GetValue<bool>("Seed:ResetToCheckpointOnStartup", false);

            if (resetOnStartup)
            {
                logger.LogInformation("Seed:ResetToCheckpointOnStartup aktif. Me-reset database ke checkpoint seeder...");
                await DataSeeder.ResetToCheckpointAsync(db, passwordHasher);
            }
            else
            {
                await DataSeeder.SeedAsync(db, passwordHasher);
            }

            // Warm up EF Core model, database connection pool, and crypto providers to eliminate first-request latency
            await WarmupServicesAsync(db, passwordHasher, logger);
        }
        catch (Exception ex)
        {
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
            _ = await db.Departments.AsNoTracking().FirstOrDefaultAsync();

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
