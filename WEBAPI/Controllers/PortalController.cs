using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Features.Portal.Queries.GetCandidateTasks;
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
}
