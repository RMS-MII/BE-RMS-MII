using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Applications.Commands.SetFinalResult;
using WEBAPI.Features.Applications.Commands.StartApplicationStage;
using WEBAPI.Features.Applications.Commands.UploadOfferingLetter;
using WEBAPI.Features.Applications.Queries.GetApplicationRecap;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Applications")]
public sealed class ApplicationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil rekap lengkap progres seluruh tahapan seleksi kandidat pada suatu batch (Khusus HR).
    /// </summary>
    /// <param name="id">ID BatchApplication</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApplicationRecapDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecap(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetApplicationRecapQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Memulai tahapan seleksi berikutnya untuk kandidat (Khusus HR).
    /// </summary>
    /// <param name="id">ID BatchApplication</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/stages/start")]
    [ProducesResponseType<ApplicationStageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartStage(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new StartApplicationStageCommand(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Menetapkan hasil akhir kelulusan rekrutmen kandidat (Accepted / Rejected) oleh HR.
    /// </summary>
    /// <param name="id">ID BatchApplication</param>
    /// <param name="request">Keputusan hasil akhir dan catatan</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPut("{id:guid}/final-result")]
    [ProducesResponseType<BatchApplicationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetFinalResult(
        [FromRoute] Guid id,
        [FromBody] SetFinalResultRequest request,
        CancellationToken ct)
    {
        var command = new SetFinalResultCommand
        {
            ApplicationId = id,
            FinalResultStatus = request.FinalResultStatus,
            Notes = request.Notes
        };

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengunggah berkas surat penawaran kerja (Offering Letter) bagi kandidat yang dinyatakan Accepted (Khusus HR).
    /// </summary>
    /// <param name="id">ID BatchApplication</param>
    /// <param name="request">Tautan berkas offering letter</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/offering-letter")]
    [ProducesResponseType<BatchApplicationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadOfferingLetter(
        [FromRoute] Guid id,
        [FromBody] UploadOfferingLetterRequest request,
        CancellationToken ct)
    {
        var command = new UploadOfferingLetterCommand
        {
            ApplicationId = id,
            FileOfferingLetter = request.FileOfferingLetter
        };

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
