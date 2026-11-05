using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBAPI.Common.Results;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Models;

namespace WEBAPI.Features.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    RmsDbContext db,
    ICurrentUser currentUser) : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var accountId = currentUser.AccountId;
        if (accountId is null)
            return Result<CurrentUserDto>.Failure(
                Error.Unauthorized("Authentication is required."));

        var user = await db.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountId && a.IsActive)
            .Select(a => new CurrentUserDto
            {
                AccountId = a.Id,
                EmployeeId = a.EmployeeId,
                Username = a.Username,
                FullName = a.Employee.FullName,
                Email = a.Employee.Email,
                Role = a.Role.Name,
                DepartmentId = a.Employee.DepartmentId,
                DepartmentName = a.Employee.Department.Name,
                JobTitle = a.Employee.JobTitle
            })
            .FirstOrDefaultAsync(ct);

        return user is null
            ? Result<CurrentUserDto>.Failure(Error.NotFound("Account was not found."))
            : Result<CurrentUserDto>.Success(user);
    }
}
