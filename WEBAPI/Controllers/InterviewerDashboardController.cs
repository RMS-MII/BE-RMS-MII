

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Interviewers.GetInterviewerDashboard;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/interviewer/dashboard")]
[Authorize(Policy = Policies.InterviewerOnly)]
[Tags("Interviews")]
public sealed class InterviewerDashboardController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil data dashboard pewawancara: ringkasan statistik, daftar task aktif yang bisa langsung dikerjakan,
    /// sesi wawancara mendatang, sesi terkini, dan undangan yang menunggu respon.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<InterviewerDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var query = new GetInterviewerDashboardQuery();
        var result = await sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
