using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Interviewers.ConfirmSession;
using WEBAPI.Features.Interviewers.SubmitAssessment;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/session-interviews")]
[Authorize(Policy = Policies.InterviewerOnly)]
public sealed class SessionInterviewsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengonfirmasi kehadiran sesi wawancara oleh pewawancara (Khusus Pewawancara).
    /// Konfirmasi ini akan mengubah status sesi menjadi AcceptedByInterviewer dan memicu pengiriman email undangan ke kandidat.
    /// </summary>
    /// <param name="id">ID SessionInterview (relasi penugasan sesi pewawancara)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType<ConfirmSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmSession(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var command = new ConfirmSessionCommand(id);
        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengisi penilaian dan rekomendasi hasil wawancara oleh pewawancara (Khusus Pewawancara).
    /// </summary>
    /// <param name="id">ID SessionInterview</param>
    /// <param name="request">Nilai, rekomendasi, dan catatan evaluasi</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/assessment")]
    [ProducesResponseType<AssessmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitAssessment(
        [FromRoute] Guid id,
        [FromBody] SubmitAssessmentRequest request,
        CancellationToken ct)
    {
        var command = new SubmitAssessmentCommand(
            id,
            request.Score,
            request.Recommendation,
            request.Notes);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
