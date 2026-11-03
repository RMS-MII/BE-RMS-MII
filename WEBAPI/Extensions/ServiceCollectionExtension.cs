using Microsoft.EntityFrameworkCore;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;

namespace WEBAPI.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRmsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RmsDB");
        services.AddDbContext<RmsDbContext>(options
            => options.UseSqlServer(connectionString));
        // services.AddScoped<IEmployeeService, EFEmployeeService>();
        //Ini opsional, karena sudah didaftarkan sendiri sendiri
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        return services;
    }
}