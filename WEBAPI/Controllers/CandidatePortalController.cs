using MediatR;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Features.Portal.UploadSubmission;
using WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/portal/{candidateId:guid}/sessions")]
public sealed class CandidatePortalController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengunggah tautan berkas Google Drive untuk tugas take-home test kandidat pada sesi wawancara.
    /// </summary>
    /// <param name="candidateId">ID kandidat pemilik sesi</param>
    /// <param name="id">ID sesi wawancara (InterviewSessionId)</param>
    /// <param name="request">Tautan berkas Google Drive</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/submission")]
    [ProducesResponseType<SubmissionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadSubmission(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] UploadSubmissionRequest request,
        CancellationToken ct)
    {
        var command = new UploadSubmissionCommand(candidateId, id, request.FileSubmission);
        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengajukan permohonan reschedule jadwal sesi wawancara oleh kandidat melalui portal kandidat.
    /// </summary>
    /// <param name="candidateId">ID kandidat pemilik sesi</param>
    /// <param name="id">ID sesi wawancara (InterviewSessionId)</param>
    /// <param name="request">Usulan jadwal baru dan alasan permohonan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/reschedule")]
    [ProducesResponseType<RescheduleRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RequestRescheduleByCandidate(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] CandidateRequestRescheduleRequest request,
        CancellationToken ct)
    {
        var command = new RequestRescheduleCommand(
            id,
            request.ProposedStart,
            request.ProposedEnd,
            request.Reason,
            candidateId);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}

