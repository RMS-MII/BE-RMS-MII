using System.Security.Claims;
using System.Text;
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
                    RoleClaimType = HrisClaimTypes.Role,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.HrOnly, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(RmsRoles.Hr) ||
                ctx.User.HasClaim(HrisClaimTypes.Role, RmsRoles.Hr) ||
                ctx.User.HasClaim(ClaimTypes.Role, RmsRoles.Hr)))
            .AddPolicy(Policies.InterviewerOnly, policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole(RmsRoles.Interviewer) ||
                ctx.User.HasClaim(HrisClaimTypes.Role, RmsRoles.Interviewer) ||
                ctx.User.HasClaim(ClaimTypes.Role, RmsRoles.Interviewer)))
            .AddPolicy(Policies.HasEmployeeProfile,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim(c => c.Type == HrisClaimTypes.EmployeeId && !string.IsNullOrWhiteSpace(c.Value))));

        return services;
    }
}
