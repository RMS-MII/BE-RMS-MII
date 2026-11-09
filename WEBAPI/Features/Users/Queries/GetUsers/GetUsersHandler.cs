using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Users.Queries.GetUsers;

public sealed class GetUsersHandler(RmsDbContext db)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<UserAccountDto>>>
{
    public async Task<Result<PagedResult<UserAccountDto>>> Handle(
        GetUsersQuery request,
        CancellationToken ct)
    {
        var query = db.Accounts
            .AsNoTracking()
            .Include(account => account.Employee)
                .ThenInclude(employee => employee.Department)
            .Include(account => account.Role)
            .AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(account => account.IsActive == request.IsActive.Value);

        var role = request.Role?.Trim();
        if (!string.IsNullOrEmpty(role))
            query = query.Where(account => account.Role.Name == role);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(account =>
                account.Username.Contains(search) ||
                account.Employee.FullName.Contains(search) ||
                account.Employee.Email.Contains(search) ||
                account.Employee.EmployeeNumber.Contains(search) ||
                account.Employee.JobTitle.Contains(search) ||
                account.Employee.Department.Name.Contains(search) ||
                account.Role.Name.Contains(search));
        }

        var totalItems = await query.CountAsync(ct);
        var users = await query
            .OrderBy(account => account.Employee.FullName)
            .ThenBy(account => account.Username)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(account => new UserAccountDto
            {
                AccountId = account.Id,
                EmployeeId = account.EmployeeId,
                Username = account.Username,
                FullName = account.Employee.FullName,
                Email = account.Employee.Email,
                EmployeeNumber = account.Employee.EmployeeNumber,
                JobTitle = account.Employee.JobTitle,
                DepartmentId = account.Employee.DepartmentId,
                DepartmentName = account.Employee.Department.Name,
                RoleId = account.RoleId,
                Role = account.Role.Name,
                IsActive = account.IsActive,
                EmployeeIsActive = account.Employee.IsActive,
                LastLoginAt = account.LastLoginAt,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            })
            .ToListAsync(ct);

        return Result<PagedResult<UserAccountDto>>.Success(
            new PagedResult<UserAccountDto>(
                users,
                request.Page,
                request.PageSize,
                totalItems));
    }
}
