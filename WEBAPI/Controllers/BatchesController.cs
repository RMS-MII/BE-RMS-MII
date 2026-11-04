using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Batches.Commands.ConfigureBatchStages;
using WEBAPI.Features.Batches.Commands.CreateBatch;
using WEBAPI.Features.Batches.Queries.GetBatchDetail;
using WEBAPI.Features.Batches.Queries.GetBatches;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/batches")]
[Authorize(Policy = Policies.HrOnly)]
public sealed class BatchesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<RecruitmentBatchDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBatches([FromQuery] GetBatchesQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecruitmentBatchDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchDetail(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBatchDetailQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPut("{id:guid}/stages")]
    [ProducesResponseType<IReadOnlyCollection<BatchStageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfigureStages(
        Guid id, [FromBody] ConfigureBatchStagesCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command with { BatchId = id }, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost]
    [ProducesResponseType<RecruitmentBatchDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBatchCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Created($"/api/batches/{result.Value.Id}", result.Value)
            : result.ToProblemDetails(this);
    }
}
