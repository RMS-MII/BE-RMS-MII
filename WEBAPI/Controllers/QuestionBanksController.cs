using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.QuestionBanks.Queries.GetQuestionBankById;
using WEBAPI.Features.QuestionBanks.Queries.GetQuestionBanks;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/question-banks")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Question Banks")]
public sealed class QuestionBanksController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar bank soal beserta jumlah soal aktif (Khusus HR).
    /// </summary>
    /// <param name="query">Parameter filter, pencarian, dan pagination</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<QuestionBankDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetQuestionBanks(
        [FromQuery] GetQuestionBanksQuery query,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil detail bank soal beserta seluruh soal dan opsi jawabannya (Khusus HR).
    /// </summary>
    /// <param name="id">ID Bank Soal</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<QuestionBankDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionBankById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetQuestionBankByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
