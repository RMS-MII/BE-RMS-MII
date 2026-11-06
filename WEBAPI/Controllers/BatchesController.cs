using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Applications.Commands.RegisterCandidates;
using WEBAPI.Features.Applications.Queries.GetApplications;
using WEBAPI.Features.Batches.Commands.ConfigureBatchStages;
using WEBAPI.Features.Batches.Commands.CreateBatch;
using WEBAPI.Features.Batches.Queries.GetBatchDetail;
using WEBAPI.Features.Batches.Queries.GetBatches;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/batches")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Batches")]
public sealed class BatchesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar batch rekrutmen dengan filter status, departemen, pencarian nama/kode, dan paginasi (Khusus HR).
    /// </summary>
    /// <param name="query">Parameter filter status, departemen, dan paginasi</param>
    /// <param name="ct">Cancellation token</param>
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

    /// <summary>
    /// Mengambil detail satu batch rekrutmen beserta seluruh tahapan seleksi yang telah dikonfigurasi (Khusus HR).
    /// </summary>
    /// <param name="id">ID Batch rekrutmen</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecruitmentBatchDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchDetail(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBatchDetailQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Membuat batch rekrutmen baru (Khusus HR). Kode batch otomatis di-generate jika field batchCode dikosongkan.
    /// </summary>
    /// <param name="command">Data pembuatan batch rekrutmen baru</param>
    /// <param name="ct">Cancellation token</param>
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

    /// <summary>
    /// Mengonfigurasi dan menyusun rantai urutan tahapan seleksi pada suatu batch (Khusus HR).
    /// </summary>
    /// <param name="id">ID Batch rekrutmen</param>
    /// <param name="request">Daftar tahapan seleksi yang disusun berurutan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPut("{id:guid}/stages")]
    [ProducesResponseType<IReadOnlyCollection<BatchStageDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfigureStages(
        Guid id, [FromBody] ConfigureBatchStagesRequest request, CancellationToken ct)
    {
        var command = new ConfigureBatchStagesCommand
        {
            BatchId = id,
            Stages = request.Stages
        };

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil seluruh pendaftaran pelamar pada batch tertentu dengan filter dan paginasi (Khusus HR).
    /// </summary>
    /// <param name="id">ID Batch rekrutmen</param>
    /// <param name="query">Parameter pencarian dan paginasi</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}/applications")]
    [ProducesResponseType<PagedResult<BatchApplicationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplications(
        Guid id, [FromQuery] GetApplicationsQuery query, CancellationToken ct)
    {
        var result = await sender.Send(query with { BatchId = id }, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mendaftarkan satu atau lebih kandidat ke dalam batch rekrutmen (Khusus HR). Kode aplikasi otomatis di-generate.
    /// </summary>
    /// <param name="id">ID Batch rekrutmen</param>
    /// <param name="request">Daftar profil kandidat yang didaftarkan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/applications")]
    [ProducesResponseType<IReadOnlyCollection<BatchApplicationDto>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegisterCandidates(
        Guid id, [FromBody] RegisterCandidatesRequest request, CancellationToken ct)
    {
        var command = new RegisterCandidatesCommand
        {
            BatchId = id,
            Candidates = request.Candidates
        };

        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Created($"/api/batches/{id}/applications", result.Value)
            : result.ToProblemDetails(this);
    }
}
