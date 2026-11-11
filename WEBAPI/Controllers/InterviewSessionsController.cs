using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Interviewers.AssignTakeHome;
using WEBAPI.Features.Interviewers.GetAvailableInterviewersForStage;
using WEBAPI.Features.Interviewers.GetSubmission;
using WEBAPI.Features.Interviewers.ScheduleInterviewSession;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/interview-sessions")]
[Tags("Interviews")]
public sealed class InterviewSessionsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar pewawancara yang sudah berstatus Accepted pada tahapan kandidat dan siap dijadwalkan ke sesi wawancara (Khusus HR).
    /// </summary>
    /// <param name="id">ID ApplicationStage kandidat</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("/api/application-stages/{id:guid}/available-interviewers")]
    [Authorize(Policy = Policies.HrOnly)]
    [ProducesResponseType<IReadOnlyCollection<AssignedInterviewerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableInterviewers(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetAvailableInterviewersForStageQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Menjadwalkan sesi wawancara untuk kandidat pada tahapan wawancara (Khusus HR).
    /// </summary>
    /// <param name="id">ID ApplicationStage kandidat</param>
    /// <param name="request">Informasi jadwal sesi dan daftar ID pewawancara (bisa dikirim via field employeeIds atau interviewerEmployeeIds)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("/api/application-stages/{id:guid}/interview-sessions")]
    [Authorize(Policy = Policies.HrOnly)]
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
            request.GetInterviewerEmployeeIds(),
            request.LeadEmployeeId);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Menetapkan tugas take-home test beserta batas waktu dan instruksi kepada kandidat (Khusus Pewawancara).
    /// </summary>
    /// <param name="id">ID InterviewSession</param>
    /// <param name="request">Batas waktu pengumpulan dan instruksi pengerjaan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/take-home")]
    [Authorize(Policy = Policies.InterviewerOnly)]
    [ProducesResponseType<AssignTakeHomeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignTakeHome(
        [FromRoute] Guid id,
        [FromBody] AssignTakeHomeRequest request,
        CancellationToken ct)
    {
        var command = new AssignTakeHomeCommand(
            id,
            request.EndedSubmissionAt,
            request.TaskTitle,
            request.Instructions,
            request.FileTakeHome);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil data pengumpulan berkas tugas take-home kandidat pada sesi wawancara (Pewawancara dan HR).
    /// </summary>
    /// <param name="id">ID InterviewSession</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}/submission")]
    [Authorize]
    [ProducesResponseType<SubmissionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubmission(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var query = new GetSubmissionQuery(id);
        var result = await sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
