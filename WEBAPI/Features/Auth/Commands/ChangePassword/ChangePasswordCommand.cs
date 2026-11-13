using MediatR;
using WEBAPI.Common.Results;

namespace WEBAPI.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}