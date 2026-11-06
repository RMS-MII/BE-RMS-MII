using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Employees.Queries.GetEmployees;

public sealed record GetEmployeesQuery(
    string? Search = null,
    Guid? DepartmentId = null,
    bool? ActiveOnly = true) : IRequest<Result<IReadOnlyCollection<EmployeeDto>>>;

