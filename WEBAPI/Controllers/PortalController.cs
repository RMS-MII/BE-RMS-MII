using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Features.Portal.Commands.SubmitTest;
using WEBAPI.Features.Portal.Queries.GetCandidateTasks;
using WEBAPI.Features.Portal.Queries.GetTest;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/portal")]
[AllowAnonymous]
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
}
