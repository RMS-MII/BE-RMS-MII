using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Features.Portal.PublicApplications;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/portal/session")]
[Authorize(Policy = CandidateSession.Policy)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Tags("Candidate session")]
public sealed class CandidateSessionController(RmsDbContext db, PublicApplicationCodes codes, CandidateProfiles profiles) : ControllerBase
{
    [HttpPost("code")]
    [AllowAnonymous]
    [EnableRateLimiting("PublicApplicationCodes")]
    public async Task<IActionResult> RequestCode([FromBody] CandidateCodeRequest request, CancellationToken ct)
    {
        if (!CandidateSession.AllowsRequest(HttpContext))
            return StatusCode(StatusCodes.Status403Forbidden);

        var result = await codes.IssueAsync(request.Email, ct);
        return result.IsSuccess ? Accepted(new { verificationId = result.Value }) : result.ToProblemDetails(this);
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    [EnableRateLimiting("PublicApplicationCodes")]
    public async Task<IActionResult> Verify([FromBody] CandidateCodeVerification request, CancellationToken ct)
    {
        if (!CandidateSession.AllowsRequest(HttpContext))
            return StatusCode(StatusCodes.Status403Forbidden);

        var result = await codes.VerifyAsync(request, ct);
        if (result.IsFailure)
            return result.ToProblemDetails(this);

        var email = result.Value;
        var candidate = await db.Candidates.AsNoTracking().SingleOrDefaultAsync(item => item.Email == email, ct);
        await HttpContext.SignInAsync(CandidateSession.Scheme, CandidateSession.Create(email, candidate?.Id));
        return Ok(new { email, profile = candidate?.ToDto() });
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var id = User.CandidateId();
        var candidate = id is null ? null : await db.Candidates.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.Email == email, ct);
        return Ok(new { email, profile = candidate?.ToDto() });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> SaveProfile([FromBody] CreateCandidateDto profile, CancellationToken ct)
    {
        var email = User.FindFirstValue(ClaimTypes.Email)!;
        var result = await profiles.SaveAsync(email, profile, ct);
        if (result.IsFailure)
            return result.ToProblemDetails(this);

        if (User.CandidateId() != result.Value.Id)
            await HttpContext.SignInAsync(CandidateSession.Scheme, CandidateSession.Create(email, result.Value.Id));
        return Ok(new { email, profile = result.Value });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        if (!CandidateSession.AllowsRequest(HttpContext))
            return StatusCode(StatusCodes.Status403Forbidden);

        await HttpContext.SignOutAsync(CandidateSession.Scheme);
        await ExamSession.SignOutAsync(HttpContext);
        return NoContent();
    }
}
