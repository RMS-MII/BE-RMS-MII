using System.Security.Claims;
using System.Text.Json.Serialization;
using WEBAPI.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting RMS API");
    var builder = WebApplication.CreateBuilder(args);

    builder.AddRmsSerilog();

    builder.Services.AddControllers()
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            if (builder.Environment.IsDevelopment())
                origins = [..origins,
                    "http://localhost:5173", "http://localhost:5174", "http://localhost:5175",
                    "http://127.0.0.1:5173", "http://127.0.0.1:5174", "http://127.0.0.1:5175"];
            policy.WithOrigins(origins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
                .AllowCredentials();
        });
    });

    builder.Services.AddRmsServices(builder.Configuration);
    builder.Services.AddRmsAuthentication(builder.Configuration);
    builder.Services.AddRmsOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await app.SeedDatabaseAsync(args);
    }

    // Outermost middleware: log every request (including 4xx/5xx, 404 and unhandled exceptions)
    // and make sure each one has a correlation id before anything else runs.
    app.UseCorrelationId();
    app.UseRmsRequestLogging();

    app.UseExceptionHandler(); 
    app.UseRmsOpenApi();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();
    app.UseAuthentication();
    app.Use(async (context, next) =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        using (Serilog.Context.LogContext.PushProperty("UserId", userId))
            await next(context);
    });
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "RMS API terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.Information("RMS API stopped");
    Log.CloseAndFlush();
}
