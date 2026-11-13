using MediatR;
using WEBAPI.Common.Results;

namespace WEBAPI.Features.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
}