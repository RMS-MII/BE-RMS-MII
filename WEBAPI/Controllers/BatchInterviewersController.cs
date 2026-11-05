using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Interviewers.AssignInterviewers;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/batches/{batchId:guid}/stages/{stageId:guid}/interviewers")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Batches")]
public sealed class BatchInterviewersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Menugaskan satu atau lebih pewawancara ke tahapan wawancara pada suatu batch (Khusus HR).
    /// </summary>
    /// <param name="batchId">ID Batch rekrutmen</param>
    /// <param name="stageId">ID Tahapan batch (harus bertipe Wawancara)</param>
    /// <param name="request">Daftar ID Karyawan yang ditugaskan sebagai pewawancara</param>
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
        var command = new AssignInterviewersCommand(batchId, stageId, request.InterviewerEmployeeIds);
        var result = await sender.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblemDetails(this);
    }
}
