using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Behaviors;
using WEBAPI.Common.ExceptionHandlers;
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
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();  
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));   // Module 7
        });
        
        return services;
    }
}