using System.Security.Claims;
using System.Text.Json.Serialization;
using WEBAPI.Extensions;
using Serilog;

// Bootstrap logger: catches errors that happen BEFORE the host is built (a broken appsettings.json, for example).
// AddHrisSerilog replaces it with the logger configured from appsettings.
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
    builder.Services.AddRmsServices(builder.Configuration);
    builder.Services.AddRmsAuthentication(builder.Configuration);
    // Program.cs
    builder.Services.AddControllers(options =>
        {
            // Without this, MVC adds an implicit [Required] to every non-nullable string and answers with its own 400 format
            // BEFORE FluentValidation runs. With it, FluentValidation is the only place that rejects missing values.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        })
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();   // class demo only; production lists the allowed origins
        });
    });

    builder.Services.AddOpenApi();                    // built-in OpenAPI document, from Module 1
    // builder.Services.AddHrisSecurity(builder.Configuration);
    builder.Services.AddRmsOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
        await app.SeedDatabaseAsync();

    // if (app.Environment.IsDevelopment())
    //     await app.SeedBootstrapAdminAsync();   // Module 10: admin@hris.local (update the database first)

    // app.UseRequestTiming();
    // app.UseHrisRequestLogging();                      // one structured line per request
    app.UseExceptionHandler(); 
    // app.UseMaintenanceMode();
    app.UseRmsOpenApi();
    app.UseHttpsRedirection();
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
catch (Exception ex) when (ex is not HostAbortedException)   // 'dotnet ef' stops the host on purpose: not a failure
{
    Log.Fatal(ex, "RMS API terminated unexpectedly");
}
finally
{
    Log.Information("RMS API stopped");
    Log.CloseAndFlush();                              // write the last buffered events before the process exits
}