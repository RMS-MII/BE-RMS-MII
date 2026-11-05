using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;
using WEBAPI.Features.RescheduleRequests.Commands.ReviewReschedule;
using WEBAPI.Features.RescheduleRequests.Queries.GetRescheduleRequests;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/reschedule-requests")]
public sealed class RescheduleRequestsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengajukan permohonan reschedule jadwal sesi wawancara oleh Pewawancara yang ditugaskan atau HR.
    /// </summary>
    /// <param name="request">Informasi sesi, usulan waktu baru, dan alasan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost]
    [Authorize]
    [ProducesResponseType<RescheduleRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RequestReschedule(
        [FromBody] RequestRescheduleRequest request,
        CancellationToken ct)
    {
        var command = new RequestRescheduleCommand(
            request.InterviewSessionId,
            request.ProposedStart,
            request.ProposedEnd,
            request.Reason);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil daftar seluruh permohonan reschedule jadwal wawancara untuk ditinjau oleh HR.
    /// </summary>
    /// <param name="status">Filter berdasarkan status permohonan (opsional: Pending, Approved, Rejected)</param>
    /// <param name="sessionId">Filter berdasarkan ID sesi wawancara tertentu (opsional)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [Authorize(Policy = Policies.HrOnly)]
    [ProducesResponseType<IReadOnlyCollection<RescheduleRequestDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRescheduleRequests(
        [FromQuery] RescheduleStatus? status,
        [FromQuery] Guid? sessionId,
        CancellationToken ct)
    {
        var query = new GetRescheduleRequestsQuery(status, sessionId);
        var result = await sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Meninjau dan memberikan keputusan (Approve atau Reject) terhadap permohonan reschedule oleh HR.
    /// </summary>
    /// <param name="id">ID permohonan reschedule (RescheduleRequestId)</param>
    /// <param name="request">Keputusan (Approved / Rejected) dan catatan peninjauan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/review")]
    [Authorize(Policy = Policies.HrOnly)]
    [ProducesResponseType<RescheduleRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReviewReschedule(
        [FromRoute] Guid id,
        [FromBody] ReviewRescheduleRequest request,
        CancellationToken ct)
    {
        var command = new ReviewRescheduleCommand(
            id,
            request.Decision,
            request.ReviewNotes);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}

