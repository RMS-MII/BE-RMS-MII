using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBAPI.Common.Results;
using WEBAPI.Features.Departments.Queries.GetDepartmentById;
using WEBAPI.Features.Departments.Queries.GetDepartments;
using WEBAPI.Models;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
[Tags("Departments")]
public sealed class DepartmentsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Mengambil seluruh data departemen untuk pilihan dropdown saat pembuatan batch atau filter.
    /// </summary>
    /// <param name="search">Pencarian opsional berdasarkan nama atau kode departemen</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<DepartmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDepartments([FromQuery] string? search, CancellationToken ct)
    {
        var result = await sender.Send(new GetDepartmentsQuery(search), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    /// <summary>
    /// Mengambil detail satu departemen berdasarkan ID.
    /// </summary>
    /// <param name="id">ID Departemen</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DepartmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetDepartmentByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }
}

