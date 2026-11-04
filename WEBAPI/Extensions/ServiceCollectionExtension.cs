using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Behaviors;
using WEBAPI.Common.ExceptionHandlers;
using WEBAPI.Data;
using WEBAPI.Data.Repositories;
using WEBAPI.Middleware;
using WEBAPI.Services;

namespace WEBAPI.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRmsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("RmsDB");
        services.AddDbContext<RmsDbContext>(options
            => options.UseSqlServer(connectionString));
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<RequestTimingMiddleware>();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            problem.Instance ??= context.HttpContext.Request.Path.Value;

            if (context.HttpContext.Items.TryGetValue("X-Correlation-Id", out var correlationId))
            {
                problem.Extensions["correlationId"] = correlationId?.ToString();
            }

            if (problem is ValidationProblemDetails && !problem.Extensions.ContainsKey(ProblemDetailsWriter.ErrorCodeKey))
            {
                problem.Extensions[ProblemDetailsWriter.ErrorCodeKey] = "MALFORMED_REQUEST";
            }
        });
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();  
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        return services;
    }
}
