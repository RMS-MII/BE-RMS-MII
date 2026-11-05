using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Features.Portal.Commands.ConfirmSchedule;
using WEBAPI.Features.Portal.Commands.SubmitTest;
using WEBAPI.Features.Portal.Queries.GetCandidateTasks;
using WEBAPI.Features.Portal.Queries.GetTest;
using WEBAPI.Features.Portal.UploadSubmission;
using WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/portal")]
[AllowAnonymous]
[Tags("Portal")]
public sealed class PortalController(ISender sender) : ControllerBase
{
    [HttpGet("{candidateId:guid}/tasks")]
    [ProducesResponseType<CandidateTasksDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateTasks(
        [FromRoute] Guid candidateId,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetCandidateTasksQuery(candidateId), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpGet("{candidateId:guid}/stages/{id:guid}/test")]
    [ProducesResponseType<CandidateTestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetTestQuery(candidateId, id), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("{candidateId:guid}/stages/{id:guid}/test/submit")]
    [ProducesResponseType<SubmitTestResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] SubmitTestCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(
            command with { CandidateId = candidateId, StageId = id }, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("{candidateId:guid}/sessions/{id:guid}/confirm")]
    [ProducesResponseType<ConfirmScheduleResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmSchedule(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new ConfirmScheduleCommand(candidateId, id), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengunggah tautan berkas Google Drive untuk tugas take-home test kandidat pada sesi wawancara.
    /// </summary>
    [HttpPost("{candidateId:guid}/sessions/{id:guid}/submission")]
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
    [HttpPost("{candidateId:guid}/sessions/{id:guid}/reschedule")]
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
