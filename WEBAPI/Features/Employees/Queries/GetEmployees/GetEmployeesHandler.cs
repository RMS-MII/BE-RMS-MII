using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Employees.Queries.GetEmployees;

public sealed class GetEmployeesHandler(RmsDbContext db)
    : IRequestHandler<GetEmployeesQuery, Result<IReadOnlyCollection<EmployeeDto>>>
{
    public async Task<Result<IReadOnlyCollection<EmployeeDto>>> Handle(
        GetEmployeesQuery request, CancellationToken ct)
    {
        var query = db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.Account)
                .ThenInclude(a => a!.Role)
            .AsQueryable();

        if (request.ActiveOnly == true)
        {
            query = query.Where(e => e.IsActive);
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == request.DepartmentId.Value);
        }

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.FullName.Contains(search) ||
                                     e.Email.Contains(search) ||
                                     e.EmployeeNumber.Contains(search) ||
                                     e.JobTitle.Contains(search));
        }

        var employees = await query
            .OrderBy(e => e.Department.Name)
            .ThenBy(e => e.FullName)
            .ToListAsync(ct);

        return Result<IReadOnlyCollection<EmployeeDto>>.Success(employees.ToDtoList());
    }
}

