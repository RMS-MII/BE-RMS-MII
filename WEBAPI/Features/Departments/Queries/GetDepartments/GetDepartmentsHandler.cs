using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Departments.Queries.GetDepartments;

public sealed class GetDepartmentsHandler(RmsDbContext db)
    : IRequestHandler<GetDepartmentsQuery, Result<IReadOnlyCollection<DepartmentDto>>>
{
    public async Task<Result<IReadOnlyCollection<DepartmentDto>>> Handle(
        GetDepartmentsQuery request, CancellationToken ct)
    {
        var query = db.Departments
            .AsNoTracking()
            .Include(d => d.ParentDepartment)
            .AsQueryable();

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Name.Contains(search) || d.Code.Contains(search));
        }

        var departments = await query
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return Result<IReadOnlyCollection<DepartmentDto>>.Success(departments.ToDtoList());
    }
}

