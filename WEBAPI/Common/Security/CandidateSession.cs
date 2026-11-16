using System.Security.Claims;

namespace WEBAPI.Common.Security;

public static class CandidateSession
{
    public const string Scheme = "CandidateSession";
    public const string Policy = "CandidatePortal";

    public static Guid? CandidateId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static bool OwnsRoute(ClaimsPrincipal user, HttpContext context) =>
        !context.Request.RouteValues.TryGetValue("candidateId", out var routeId) ||
        (Guid.TryParse(routeId?.ToString(), out var id) && user.CandidateId() == id);

    public static bool AllowsRequest(HttpContext context)
    {
        var request = context.Request;
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
            return true;

        var origin = request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
            return request.Headers["Sec-Fetch-Site"] != "cross-site";

        var services = context.RequestServices;
        if (origin == $"{request.Scheme}://{request.Host}")
            return true;

        var configured = services.GetRequiredService<IConfiguration>()
            .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        return configured.Contains(origin, StringComparer.OrdinalIgnoreCase) ||
            (services.GetRequiredService<IHostEnvironment>().IsDevelopment() &&
             new[] { "http://localhost:5175", "http://localhost:5174", "http://127.0.0.1:5175", "http://127.0.0.1:5174" }
                 .Contains(origin, StringComparer.OrdinalIgnoreCase));
    }

    public static ClaimsPrincipal Create(string email, Guid? candidateId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Email, email) };
        if (candidateId is { } id)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
    }
}
