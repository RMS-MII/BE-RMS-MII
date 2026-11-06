using Serilog.Context;

namespace WEBAPI.Middleware;

/// <summary>
/// Ensures every request has an X-Correlation-Id: reads it from the request (or creates one),
/// stores it in HttpContext.Items, echoes it on the response, and pushes it into the Serilog
/// LogContext so every log line written for the request carries the same id.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationHeader = "X-Correlation-Id";
    private const string CorrelationItemKey = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[CorrelationItemKey] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        // UseExceptionHandler clears the response headers before it writes an error response.
        // Keep the id in Items and put the header back right before the response starts.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
