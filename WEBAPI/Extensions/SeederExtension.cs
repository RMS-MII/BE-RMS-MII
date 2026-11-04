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
            await DataSeeder.SeedAsync(db, passwordHasher);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seeder gagal dijalankan");
        }
    }
}
