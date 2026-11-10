using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.ApplicationStages.Commands.EvaluateStage;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/application-stages")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Applications")]
public sealed class ApplicationStagesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Menilai hasil tahapan seleksi kandidat (Prioritized / Considered / Rejected / Dropped) oleh HR.
    /// </summary>
    /// <param name="id">ID ApplicationStage kandidat</param>
    /// <param name="request">Status kelulusan dan catatan evaluasi</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/evaluate")]
    [ProducesResponseType<ApplicationStageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Evaluate(
        [FromRoute] Guid id,
        [FromBody] EvaluateStageRequest request,
        CancellationToken ct)
    {
        var command = new EvaluateStageCommand
        {
            StageId = id,
            Status = request.Status,
            Notes = request.Notes
        };

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
