using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Users.Queries.GetUsers;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil daftar akun RMS beserta profil pegawai, role, dan status akun (Khusus HR).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<UserAccountDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] GetUsersQuery query,
        CancellationToken ct)
    {
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}
