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
            policy.AllowAnyOrigin();
        });
    });

    builder.Services.AddRmsServices(builder.Configuration);
    builder.Services.AddRmsAuthentication(builder.Configuration);
    builder.Services.AddRmsOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await app.SeedDatabaseAsync();
    }

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
}
finally
{
    Log.Information("RMS API stopped");
    Log.CloseAndFlush();
}
