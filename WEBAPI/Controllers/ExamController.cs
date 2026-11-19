using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Portal.Commands.FinishTest;
using WEBAPI.Features.Portal.Commands.SubmitAnswer;
using WEBAPI.Features.Portal.Queries.GetTest;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/exam/{candidateId:guid}/stages/{stageId:guid}/test")]
[Authorize(Policy = ExamSession.Policy)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Tags("Exam")]
public sealed class ExamController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CandidateTestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid stageId,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetTestQuery(candidateId, stageId, IncludeQuestions: true), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("answers")]
    [ProducesResponseType<SubmitAnswerResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitAnswer(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid stageId,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken ct)
    {
        var command = new SubmitAnswerCommand
        {
            CandidateId = candidateId,
            StageId = stageId,
            Answers = request.Answers,
            IsDraft = request.IsDraft
        };

        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("development/answers/{outcome}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GenerateDevelopmentAnswers(
        Guid candidateId, Guid stageId, string outcome,
        [FromServices] IWebHostEnvironment environment, CancellationToken ct)
    {
        if (!environment.IsDevelopment() || outcome is not ("pass" or "fail"))
            return NotFound();

        var result = await sender.Send(new SubmitAnswerCommand
        {
            CandidateId = candidateId,
            StageId = stageId,
            DevelopmentPass = outcome == "pass"
        }, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("finish")]
    [ProducesResponseType<FinishTestResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FinishTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid stageId,
        CancellationToken ct)
    {
        var result = await sender.Send(new FinishTestCommand(candidateId, stageId), ct);
        if (result.IsFailure)
            return result.ToProblemDetails(this);

        await ExamSession.SignOutAsync(HttpContext);
        return Ok(result.Value);
    }
}
