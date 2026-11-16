using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WEBAPI.Common.Security;
using WEBAPI.Entities;

namespace WEBAPI.Extensions;

public static class AuthExtension
{
    public static IServiceCollection AddRmsAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(section);
        services.Configure<RefreshSessionOptions>(configuration.GetSection(RefreshSessionOptions.SectionName));

        var settings = section.Get<JwtSettings>()
            ?? throw new InvalidOperationException("Konfigurasi 'Jwt' tidak ditemukan.");

        if (string.IsNullOrWhiteSpace(settings.Key))
        {
            throw new InvalidOperationException(
                "Jwt:Key belum diatur. Gunakan user-secrets atau environment variable (Jwt__Key).");
        }

        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(settings.Key)),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            })
            .AddCookie(CandidateSession.Scheme, options =>
            {
                options.Cookie.Name = "rms_candidate";
                options.Cookie.Path = "/api/portal";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = false;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddCookie(ExamSession.Scheme, options =>
            {
                options.Cookie.Name = ExamSession.CookieName;
                options.Cookie.Path = ExamSession.CookiePath;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = false;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(CandidateSession.Policy, policy => policy
                .AddAuthenticationSchemes(CandidateSession.Scheme)
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.Email)
                .RequireAssertion(context => context.Resource is HttpContext http &&
                    CandidateSession.OwnsRoute(context.User, http) &&
                    CandidateSession.AllowsRequest(http)))
            .AddPolicy(ExamSession.Policy, policy => policy
                .AddAuthenticationSchemes(ExamSession.Scheme)
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.NameIdentifier)
                .RequireAssertion(context => context.Resource is HttpContext http &&
                    ExamSession.OwnsRoute(context.User, http) &&
                    CandidateSession.AllowsRequest(http)))
            .AddPolicy(Policies.HrOnly, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(RmsRoles.Hr) ||
                ctx.User.HasClaim(HrisClaimTypes.Role, RmsRoles.Hr) ||
                ctx.User.HasClaim(ClaimTypes.Role, RmsRoles.Hr)))
            .AddPolicy(Policies.InterviewerOnly, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(RmsRoles.Interviewer) ||
                ctx.User.HasClaim(HrisClaimTypes.Role, RmsRoles.Interviewer) ||
                ctx.User.HasClaim(ClaimTypes.Role, RmsRoles.Interviewer) ||
                ctx.User.IsInRole(RmsRoles.Hr) ||
                ctx.User.HasClaim(HrisClaimTypes.Role, RmsRoles.Hr) ||
                ctx.User.HasClaim(ClaimTypes.Role, RmsRoles.Hr)))
            .AddPolicy(Policies.HasEmployeeProfile,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == HrisClaimTypes.EmployeeId && !string.IsNullOrWhiteSpace(c.Value))));

        return services;
    }
}
