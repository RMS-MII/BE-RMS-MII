using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Interviewers.GetInterviewerSessions;
using WEBAPI.Features.Interviewers.GetInterviewerSessionDetail;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/interviewer/sessions")]
[Authorize(Policy = Policies.InterviewerOnly)]
[Tags("Interviews")]
public sealed class InterviewerSessionsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil detail sesi, profil kandidat, tugas take-home, dan penilaian milik pewawancara yang ditugaskan.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<InterviewerSessionDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionDetail(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetInterviewerSessionDetailQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil seluruh jadwal sesi wawancara kandidat yang di-assign ke akun pewawancara yang sedang login.
    /// Endpoint ini digunakan untuk melihat detail jadwal dan menentukan aksi konfirmasi hadir (confirm) atau permohonan ganti jadwal (reschedule).
    /// </summary>
    /// <param name="status">Filter status wawancara: 1 = InProgress, 2 = Completed, 3 = Cancelled</param>
    /// <param name="sessionStatus">Filter status sesi: 1 = Scheduled, 2 = PendingConfirmation, 3 = Rescheduled, 4 = AcceptedByInterviewer, 5 = AcceptedByCandidate</param>
    /// <param name="from">Filter tanggal mulai sesi dari waktu ini</param>
    /// <param name="to">Filter tanggal mulai sesi hingga waktu ini</param>
    /// <param name="pendingActionOnly">Jika true, hanya tampilkan sesi yang masih menunggu konfirmasi pewawancara (Status sesi: PendingConfirmation)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<DashboardInterviewSessionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSessions(
        [FromQuery] SessionInterviewStatus? status,
        [FromQuery] InterviewSessionStatus? sessionStatus,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool? pendingActionOnly,
        CancellationToken ct)
    {
        var query = new GetInterviewerSessionsQuery(status, sessionStatus, from, to, pendingActionOnly);
        var result = await sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
