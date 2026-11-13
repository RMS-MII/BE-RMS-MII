using MediatR;
using WEBAPI.Common.Results;

namespace WEBAPI.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommand : IRequest<Result>
{
    public string Token { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}