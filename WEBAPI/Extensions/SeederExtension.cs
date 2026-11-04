using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Data.Seeders;

namespace WEBAPI.Extensions;

public static class SeederExtension
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
            if (!await db.Database.CanConnectAsync())
            {
                logger.LogWarning("Database tidak dapat dihubungi; seeder dilewati");
                return;
            }

            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seeder gagal dijalankan");
        }
    }
}
