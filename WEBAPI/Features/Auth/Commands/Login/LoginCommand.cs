using MediatR;
using WEBAPI.Common.Results;
using WEBAPI.Models;

namespace WEBAPI.Features.Auth.Commands.Login;

public sealed record LoginCommand : IRequest<Result<LoginResponseDto>>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
