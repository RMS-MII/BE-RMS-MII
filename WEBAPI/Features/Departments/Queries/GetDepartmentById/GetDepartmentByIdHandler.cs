using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Departments.Queries.GetDepartmentById;

public sealed class GetDepartmentByIdHandler(RmsDbContext db)
    : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDto>>
{
    public async Task<Result<DepartmentDto>> Handle(
        GetDepartmentByIdQuery request, CancellationToken ct)
    {
        var department = await db.Departments
            .AsNoTracking()
            .Include(d => d.ParentDepartment)
            .FirstOrDefaultAsync(d => d.Id == request.Id, ct);

        if (department is null)
        {
            return Result<DepartmentDto>.Failure(
                Error.NotFound($"Department with ID '{request.Id}' was not found."));
        }

        return Result<DepartmentDto>.Success(department.ToDto());
    }
}
