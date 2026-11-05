using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Departments.Queries.GetDepartments;

public sealed record GetDepartmentsQuery(string? Search = null) : IRequest<Result<IReadOnlyCollection<DepartmentDto>>>;

