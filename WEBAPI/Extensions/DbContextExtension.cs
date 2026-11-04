using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Entities;

namespace WEBAPI.Extensions;

public static class DbContextExtension
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RmsDB");
        services.AddDbContext<RmsDbContext>(options => options.UseSqlServer(connectionString));
        
        return services;
    }

    public static async Task SeedRmsDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();
        await DataSeeder.SeedAsync(dbContext, hasher);
    }
}
