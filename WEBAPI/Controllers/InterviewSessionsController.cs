using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Interviewers.ScheduleInterviewSession;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/application-stages/{id:guid}/interview-sessions")]
[Authorize(Policy = Policies.HrOnly)]
public sealed class InterviewSessionsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Menjadwalkan sesi wawancara untuk kandidat pada tahapan wawancara (Khusus HR).
    /// </summary>
    /// <param name="id">ID ApplicationStage kandidat</param>
    /// <param name="request">Informasi jadwal sesi dan daftar ID pewawancara</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost]
    [ProducesResponseType<InterviewSessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ScheduleInterviewSession(
        [FromRoute] Guid id,
        [FromBody] ScheduleInterviewSessionRequest request,
        CancellationToken ct)
    {
        var command = new ScheduleInterviewSessionCommand(
            id,
            request.SessionName,
            request.ScheduledStart,
            request.ScheduledEnd,
            request.IsOnline,
            request.Location,
            request.MeetingUrl,
            request.InterviewerEmployeeIds);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
