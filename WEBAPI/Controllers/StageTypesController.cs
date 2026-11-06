using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.StageTypes.Commands.CreateStageType;
using WEBAPI.Features.StageTypes.Queries.GetStageTypeById;
using WEBAPI.Features.StageTypes.Queries.GetStageTypes;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/stage-types")]
[Authorize]
[Tags("Stage Types")]
public sealed class StageTypesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil seluruh tipe tahapan seleksi (Bisa diakses oleh HR dan Interviewer).
    /// </summary>
    /// <param name="category">Filter kategori: 1 = Test, 2 = Interview, 3 = Offering</param>
    /// <param name="activeOnly">Hanya ambil yang aktif (opsional)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<StageTypeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStageTypes(
        [FromQuery] StageCategory? category,
        [FromQuery] bool? activeOnly,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetStageTypesQuery(category, activeOnly), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil detail satu tipe tahapan seleksi berdasarkan ID.
    /// </summary>
    /// <param name="id">ID Tipe Tahapan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<StageTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStageTypeById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetStageTypeByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Menambahkan tipe tahapan seleksi baru (Khusus HR).
    /// </summary>
    /// <param name="command">Data tipe tahapan baru</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost]
    [Authorize(Policy = Policies.HrOnly)]
    [ProducesResponseType<StageTypeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateStageType(
        [FromBody] CreateStageTypeCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetStageTypeById), new { id = result.Value.Id }, result.Value)
            : result.ToProblemDetails(this);
    }
}

