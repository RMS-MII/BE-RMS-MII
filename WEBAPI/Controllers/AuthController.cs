using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WEBAPI.Common.Security;
using WEBAPI.Common.Results;
using WEBAPI.Features.Auth.Commands;
using WEBAPI.Features.Auth.Commands.ChangePassword;
using WEBAPI.Features.Auth.Commands.ForgotPassword;
using WEBAPI.Features.Auth.Commands.Login;
using WEBAPI.Features.Auth.Commands.ResetPassword;
using WEBAPI.Features.Auth.Queries.GetCurrentUser;
using WEBAPI.Models;
using WEBAPI.Services;

namespace WEBAPI.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public sealed class AuthController(
    ISender sender,
    RefreshSessionService refreshSessions,
    IJwtTokenService jwtTokens,
    IOptions<RefreshSessionOptions> refreshOptions,
    IConfiguration configuration) : ControllerBase
{
    private readonly RefreshSessionOptions _refreshOptions = refreshOptions.Value;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        if (!IsAllowedOrigin()) return Forbid();
        var result = await sender.Send(command, ct);
        if (!result.IsSuccess) return result.ToProblemDetails(this);
        await refreshSessions.RevokeAsync(Request.Cookies[_refreshOptions.CookieName], ct);
        var refreshToken = await refreshSessions.CreateAsync(result.Value!.User.AccountId, ct);
        SetRefreshCookie(refreshToken);
        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!IsAllowedOrigin()) return Forbid();
        var rawToken = Request.Cookies[_refreshOptions.CookieName];
        var rotation = await refreshSessions.RotateAsync(rawToken ?? string.Empty, ct);
        if (rotation.IsConcurrent) return Conflict();
        if (rotation.Session is not { } rotated)
        {
            DeleteRefreshCookie();
            return Unauthorized();
        }
        var (accessToken, expiresAt) = jwtTokens.GenerateToken(rotated.Account);
        SetRefreshCookie(rotated.Token);
        return Ok(rotated.Account.ToLoginResponse(accessToken, expiresAt));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!IsAllowedOrigin()) return Forbid();
        await refreshSessions.RevokeAsync(Request.Cookies[_refreshOptions.CookieName], ct);
        DeleteRefreshCookie();
        return NoContent();
    }

    private void SetRefreshCookie(IssuedRefreshToken token)
    {
        var sameSite = Enum.TryParse<SameSiteMode>(_refreshOptions.SameSite, true, out var parsed)
            ? parsed : SameSiteMode.Lax;
        Response.Cookies.Append(_refreshOptions.CookieName, token.Value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Path = "/api/auth",
            Expires = new DateTimeOffset(token.ExpiresAt, TimeSpan.Zero)
        });
    }

    private void DeleteRefreshCookie() => Response.Cookies.Delete(_refreshOptions.CookieName,
        new CookieOptions { Secure = true, Path = "/api/auth" });

    private bool IsAllowedOrigin()
    {
        var origin = Request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
            return !string.Equals(Request.Headers["Sec-Fetch-Site"], "cross-site", StringComparison.OrdinalIgnoreCase);
        var ownOrigin = $"{Request.Scheme}://{Request.Host}";
        return string.Equals(origin, ownOrigin, StringComparison.OrdinalIgnoreCase) ||
            (configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
                .Contains(origin, StringComparer.OrdinalIgnoreCase) ||
            (HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() &&
                new[] { "http://localhost:5173", "http://127.0.0.1:5173" }
                    .Contains(origin, StringComparer.OrdinalIgnoreCase));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentUserQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(this);
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : result.ToProblemDetails(this);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(new
        {
            message = "If the email registered, a reset link will be sent."
        }) : result.ToProblemDetails(this);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? NoContent() : result.ToProblemDetails(this);
    }
}
