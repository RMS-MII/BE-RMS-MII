using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;

namespace WEBAPI.Extensions;

public static class DbContextExtension
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HrisDB");
        services.AddDbContext<RmsDbContext>(options => options.UseSqlServer(connectionString));
        
        return services;
    }
}