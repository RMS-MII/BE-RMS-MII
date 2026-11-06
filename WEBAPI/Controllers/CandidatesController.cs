using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Candidates.Queries.GetCandidateById;
using WEBAPI.Features.Candidates.Queries.GetCandidates;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/candidates")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Candidates")]
public sealed class CandidatesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar seluruh kandidat dalam sistem beserta jumlah total dan batch yang di-assign (Khusus HR).
    /// </summary>
    /// <param name="query">Parameter pencarian, filter batch, dan pagination</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<CandidateWithBatchesDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] GetCandidatesQuery query,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil detail satu kandidat beserta seluruh riwayat batch yang dilamar (Khusus HR).
    /// </summary>
    /// <param name="id">ID Kandidat</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CandidateWithBatchesDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetCandidateByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}

