using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace WEBAPI.Common.Security;

public static class ExamSession
{
    public const string Scheme = "ExamSession";
    public const string Policy = "CandidateExam";
    public const string CookieName = "rms_exam";
    public const string CookiePath = "/api/exam";

    private const string StageIdClaim = "exam_stage_id";

    public static ClaimsPrincipal Create(Guid candidateId, Guid applicationStageId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, candidateId.ToString()),
            new Claim(StageIdClaim, applicationStageId.ToString())
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));
    }

    public static bool OwnsRoute(ClaimsPrincipal user, HttpContext context) =>
        context.Request.RouteValues.TryGetValue("candidateId", out var candidateRoute) &&
        Guid.TryParse(candidateRoute?.ToString(), out var candidateId) &&
        context.Request.RouteValues.TryGetValue("stageId", out var stageRoute) &&
        Guid.TryParse(stageRoute?.ToString(), out var applicationStageId) &&
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var sessionCandidateId) &&
        Guid.TryParse(user.FindFirstValue(StageIdClaim), out var sessionStageId) &&
        candidateId == sessionCandidateId && applicationStageId == sessionStageId;

    public static Task SignInAsync(
        HttpContext context,
        Guid candidateId,
        Guid applicationStageId,
        DateTime deadlineUtc) =>
        context.SignInAsync(Scheme, Create(candidateId, applicationStageId),
            new AuthenticationProperties
            {
                IsPersistent = true,
                // Keep the receipt accessible briefly while answer writes still enforce the deadline.
                ExpiresUtc = new DateTimeOffset(DateTime.SpecifyKind(deadlineUtc, DateTimeKind.Utc)).AddMinutes(2)
            });

    public static Task SignOutAsync(HttpContext context) => context.SignOutAsync(Scheme);
}
