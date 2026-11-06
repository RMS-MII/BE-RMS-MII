using System.Security.Claims;
using Serilog;
using Serilog.Events;

namespace WEBAPI.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Replace the default logging providers with serilog, configured from the "Serilog" section of appsetting.json
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddRmsSerilog(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
            Serilog.Debugging.SelfLog.Enable(Console.Error);
        
        builder.Services.AddSerilog((services, configuration) => configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environtment", builder.Environment.EnvironmentName));
        
        return builder;
    }
    
    /// <summary>
    /// One structured line per request, written when the response is finished, so it knows the final status code.
    /// Register it near the top of the pipeline, and in Module 7 Before UseExceptionHandler
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRmsRequestLogging(this IApplicationBuilder app)
        => app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";
                options.GetLevel = GetLevel;
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    // Runs at the END of the request.
                    diagnosticContext.Set("UserId",
                        httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");

                    // Prefer Items: UseExceptionHandler may clear response headers before writing an error response.
                    if (httpContext.Items.TryGetValue("X-Correlation-Id", out var correlationId) &&
                        correlationId is not null)
                    {
                        diagnosticContext.Set("correlationId", correlationId.ToString());
                    }

                    // Not logged on purpose: bodies, query string, Authorization header, client IP (personal data and secrets)
                };
            });

    /// <summary>
    /// 5xx = Error (our bug), 4xx = Warning (client problem), everything else = information.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="elapsedMs"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    private static LogEventLevel GetLevel(HttpContext httpContext, double elapsedMs, Exception? exception)
        => exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest
                ? LogEventLevel.Warning
                : LogEventLevel.Information;


}