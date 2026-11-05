using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.ApplicationStages.Commands.EvaluateStage;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/application-stages")]
[Authorize(Policy = Policies.HrOnly)]
public sealed class ApplicationStagesController(ISender sender) : ControllerBase
{
    [HttpPost("{id:guid}/evaluate")]
    [ProducesResponseType<ApplicationStageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Evaluate(
        Guid id, [FromBody] EvaluateStageCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command with { StageId = id }, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
