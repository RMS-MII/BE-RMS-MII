using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Features.Employees.Queries.GetEmployees;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize(Policy = Policies.HrOnly)]
[Tags("Employees")]
public sealed class EmployeesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil seluruh data karyawan aktif di perusahaan untuk penugasan pewawancara atau kebutuhan HR.
    /// </summary>
    /// <param name="search">Pencarian berdasarkan nama, email, nomor karyawan, atau jabatan</param>
    /// <param name="departmentId">Filter berdasarkan ID departemen tertentu</param>
    /// <param name="activeOnly">Hanya ambil karyawan yang berstatus aktif (default: true)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<EmployeeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] Guid? departmentId,
        [FromQuery] bool? activeOnly,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeesQuery(search, departmentId, activeOnly), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}

