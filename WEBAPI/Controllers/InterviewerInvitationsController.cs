using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Entities.Enums;
using WEBAPI.Features.Interviewers.GetInvitations;
using WEBAPI.Features.Interviewers.RespondInvitation;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/interviewer/invitations")]
[Authorize(Policy = Policies.InterviewerOnly)]
[Tags("Interviews")]
public sealed class InterviewerInvitationsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar undangan penugasan tahapan wawancara untuk pewawancara yang sedang login.
    /// </summary>
    /// <param name="status">Filter status penugasan (Pending, Accepted, Declined)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<List<InterviewerInvitationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetInvitations(
        [FromQuery] StageEmployeeStatus? status,
        CancellationToken ct)
    {
        var query = new GetInvitationsQuery(status);
        var result = await sender.Send(query, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Merespons undangan penugasan wawancara (Menerima atau Menolak dengan alasan).
    /// </summary>
    /// <param name="id">ID Undangan Penugasan (StageEmployeeId)</param>
    /// <param name="request">Respon persetujuan dan catatan alasan jika menolak</param>
    /// <param name="ct">Cancellation token</param>
    [HttpPost("{id:guid}/respond")]
    [ProducesResponseType<RespondInvitationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RespondInvitation(
        [FromRoute] Guid id,
        [FromBody] RespondInvitationRequest request,
        CancellationToken ct)
    {
        var command = new RespondInvitationCommand(id, request.Response, request.Note);
        var result = await sender.Send(command, ct);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
