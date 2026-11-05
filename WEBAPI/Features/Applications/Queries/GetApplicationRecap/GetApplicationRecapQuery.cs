using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Applications.Queries.GetApplicationRecap;

public sealed record GetApplicationRecapQuery(Guid Id) : IRequest<Result<ApplicationRecapDto>>;
