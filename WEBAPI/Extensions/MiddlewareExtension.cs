using WEBAPI.Middleware;

namespace WEBAPI.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder) =>
        builder.UseMiddleware<RequestTimingMiddleware>();
}