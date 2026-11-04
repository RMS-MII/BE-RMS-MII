using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Auth.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>;
