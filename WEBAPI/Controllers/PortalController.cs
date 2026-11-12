using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Features.Applications.Commands.RegisterCandidates;
using WEBAPI.Features.Portal.Commands.ConfirmSchedule;
using WEBAPI.Features.Portal.Commands.FinishTest;
using WEBAPI.Features.Portal.Commands.StartTest;
using WEBAPI.Features.Portal.Commands.SubmitAnswer;
using WEBAPI.Features.Portal.Commands.SubmitTest;
using WEBAPI.Features.Portal.Queries.GetCandidateTasks;
using WEBAPI.Features.Portal.Queries.GetPublicJobs;
using WEBAPI.Features.Portal.Queries.GetTest;
using WEBAPI.Features.Portal.UploadSubmission;
using WEBAPI.Features.RescheduleRequests.Commands.RequestReschedule;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/portal")]
[AllowAnonymous]
[Tags("Portal")]
public sealed class PortalController(ISender sender) : ControllerBase
{
    [HttpGet("jobs")]
    public async Task<ActionResult<IReadOnlyList<PublicJobDto>>> GetJobs(CancellationToken ct) =>
        Ok(await sender.Send(new GetPublicJobsQuery(), ct));

    [HttpPost("jobs/{batchId:guid}/applications")]
    public async Task<IActionResult> Apply(
        Guid batchId, [FromBody] CreateCandidateDto candidate, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterCandidatesCommand
        {
            BatchId = batchId,
            Candidates = [candidate],
            IsPublicApplication = true
        }, ct);

        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created) : result.ToProblemDetails(this);
    }

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

    /// <summary>
    /// Mencatat waktu kandidat benar-benar mulai mengerjakan tes. Durasi tes
    /// dihitung dari waktu ini, bukan dari jadwal stage.
    /// </summary>
    /// <param name="candidateId">ID unik kandidat</param>
    /// <param name="id">ID Tahapan seleksi (ApplicationStageId)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{candidateId:guid}/stages/{id:guid}/test/start")]
    [ProducesResponseType<StartTestResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new StartTestCommand(candidateId, id), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengirimkan seluruh jawaban tes daring kandidat untuk dihitung skornya.
    /// </summary>
    /// <param name="candidateId">ID unik kandidat</param>
    /// <param name="id">ID Tahapan seleksi (ApplicationStageId)</param>
    /// <param name="request">Daftar jawaban per butir soal</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{candidateId:guid}/stages/{id:guid}/test/submit")]
    [ProducesResponseType<SubmitTestResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] SubmitTestRequest request,
        CancellationToken ct)
    {
        var command = new SubmitTestCommand
        {
            CandidateId = candidateId,
            StageId = id,
            Answers = request.Answers
        };

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengirimkan jawaban tes daring kandidat per butir soal. Server menolak
    /// request jika kandidat sudah melewati batas waktu pengerjaan yang ditetapkan
    /// pada QuestionBank (berdasarkan timestamp server).
    /// </summary>
    /// <param name="candidateId">ID unik kandidat</param>
    /// <param name="id">ID Tahapan seleksi (ApplicationStageId)</param>
    /// <param name="request">Daftar jawaban per butir soal</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{candidateId:guid}/stages/{id:guid}/test/answers")]
    [ProducesResponseType<SubmitAnswerResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitAnswer(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken ct)
    {
        var command = new SubmitAnswerCommand
        {
            CandidateId = candidateId,
            StageId = id,
            Answers = request.Answers
        };

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Menutup sesi attempt tes dan memicu perhitungan skor dari jawaban yang telah
    /// dikirim kandidat (dipicu tombol "Selesai" atau penutupan otomatis saat batas
    /// waktu habis).
    /// </summary>
    /// <param name="candidateId">ID unik kandidat</param>
    /// <param name="id">ID Tahapan seleksi (ApplicationStageId)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{candidateId:guid}/stages/{id:guid}/test/finish")]
    [ProducesResponseType<FinishTestResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> FinishTest(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new FinishTestCommand(candidateId, id), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("{candidateId:guid}/sessions/{id:guid}/confirm")]
    [ProducesResponseType<ConfirmScheduleResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConfirmSchedule(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await sender.Send(new ConfirmScheduleCommand(candidateId, id), ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengunggah tautan berkas Google Drive untuk tugas take-home test kandidat pada sesi wawancara.
    /// </summary>
    [HttpPost("{candidateId:guid}/sessions/{id:guid}/submission")]
    [ProducesResponseType<SubmissionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadSubmission(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] UploadSubmissionRequest request,
        CancellationToken ct)
    {
        var command = new UploadSubmissionCommand(candidateId, id, request.FileSubmission);
        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengajukan permohonan reschedule jadwal sesi wawancara oleh kandidat melalui portal kandidat.
    /// </summary>
    [HttpPost("{candidateId:guid}/sessions/{id:guid}/reschedule")]
    [ProducesResponseType<RescheduleRequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RequestRescheduleByCandidate(
        [FromRoute] Guid candidateId,
        [FromRoute] Guid id,
        [FromBody] CandidateRequestRescheduleRequest request,
        CancellationToken ct)
    {
        var command = new RequestRescheduleCommand(
            id,
            request.ProposedStart,
            request.ProposedEnd,
            request.Reason,
            candidateId);

        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
