using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Interviewers.AssignInterviewers;
using WEBAPI.Features.Interviewers.GetStageInterviewers;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/batches/{batchId:guid}/stages/{stageId:guid}/interviewers")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Batches")]
public sealed class BatchInterviewersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar pewawancara yang ditugaskan pada tahapan batch tertentu (Khusus HR).
    /// </summary>
    /// <param name="batchId">ID Batch rekrutmen</param>
    /// <param name="stageId">ID Tahapan batch</param>
    /// <param name="status">Filter status penugasan: 1 = Pending, 2 = Accepted, 3 = Declined</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<AssignInterviewersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInterviewers(
        [FromRoute] Guid batchId,
        [FromRoute] Guid stageId,
        [FromQuery] StageEmployeeStatus? status,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetStageInterviewersQuery(batchId, stageId, status), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Menugaskan satu atau lebih karyawan sebagai pewawancara ke tahapan wawancara pada suatu batch (Khusus HR).
    /// </summary>
    /// <param name="batchId">ID Batch rekrutmen</param>
    /// <param name="stageId">ID Tahapan batch (harus bertipe Wawancara)</param>
    /// <param name="request">Daftar ID Karyawan yang ditugaskan (dapat dikirim lewat field employeeIds atau interviewerEmployeeIds)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost]
    [ProducesResponseType<AssignInterviewersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignInterviewers(
        [FromRoute] Guid batchId,
        [FromRoute] Guid stageId,
        [FromBody] AssignInterviewersRequest request,
        CancellationToken ct)
    {
        var command = new AssignInterviewersCommand(batchId, stageId, request.GetEmployeeIds());
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails(this);
    }
}
