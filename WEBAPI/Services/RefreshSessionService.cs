using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WEBAPI.Common.Security;
using WEBAPI.Data;
using WEBAPI.Entities;

namespace WEBAPI.Services;

public sealed record IssuedRefreshToken(string Value, DateTime ExpiresAt);
public sealed record RotatedRefreshToken(Account Account, IssuedRefreshToken Token);
public sealed record RefreshRotationResult(RotatedRefreshToken? Session, bool IsConcurrent = false);

public sealed class RefreshSessionService(
    RmsDbContext db,
    IDateTimeProvider clock,
    IOptions<RefreshSessionOptions> settings)
{
    private readonly RefreshSessionOptions _settings = settings.Value;

    public async Task<IssuedRefreshToken> CreateAsync(Guid accountId, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var absoluteExpiry = now.AddDays(_settings.AbsoluteDays);
        var issued = NewToken(now, absoluteExpiry);
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = issued.Id,
            FamilyId = issued.Id,
            AccountId = accountId,
            TokenHash = Hash(issued.Value),
            CreatedAt = now,
            ExpiresAt = issued.ExpiresAt,
            AbsoluteExpiresAt = absoluteExpiry
        });
        await db.SaveChangesAsync(ct);
        return new IssuedRefreshToken(issued.Value, issued.ExpiresAt);
    }

    public async Task<RefreshRotationResult> RotateAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return new(null);
        var now = clock.GetUtcNow().UtcDateTime;
        var token = await db.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == Hash(rawToken), ct);
        if (token is null) return new(null);

        if (token.UsedAt is not null)
        {
            if (token.UsedAt.Value.AddSeconds(2) > now) return new(null, true);
            await RevokeFamilyAsync(token.FamilyId, now, ct);
            return new(null);
        }
        if (token.RevokedAt is not null || token.ExpiresAt <= now || token.AbsoluteExpiresAt <= now)
            return new(null);

        var account = await db.Accounts
            .Include(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == token.AccountId && x.IsActive && x.Employee.IsActive, ct);
        if (account is null)
        {
            await RevokeFamilyAsync(token.FamilyId, now, ct);
            return new(null);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var claimed = await db.RefreshTokens
            .Where(x => x.Id == token.Id && x.UsedAt == null && x.RevokedAt == null &&
                        x.ExpiresAt > now && x.AbsoluteExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsedAt, now), ct);
        if (claimed != 1)
        {
            await transaction.RollbackAsync(ct);
            return new(null, true);
        }

        var issued = NewToken(now, token.AbsoluteExpiresAt);
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = issued.Id,
            FamilyId = token.FamilyId,
            AccountId = token.AccountId,
            TokenHash = Hash(issued.Value),
            CreatedAt = now,
            ExpiresAt = issued.ExpiresAt,
            AbsoluteExpiresAt = token.AbsoluteExpiresAt
        });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new(new RotatedRefreshToken(account, new IssuedRefreshToken(issued.Value, issued.ExpiresAt)));
    }

    public async Task RevokeAsync(string? rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return;
        var familyId = await db.RefreshTokens.AsNoTracking()
            .Where(x => x.TokenHash == Hash(rawToken))
            .Select(x => (Guid?)x.FamilyId)
            .FirstOrDefaultAsync(ct);
        if (familyId is { } id)
            await RevokeFamilyAsync(id, clock.GetUtcNow().UtcDateTime, ct);
    }

    public Task RevokeAllForAccountAsync(Guid accountId, CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return db.RefreshTokens.Where(x => x.AccountId == accountId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), ct);
    }

    private Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken ct) =>
        db.RefreshTokens.Where(x => x.FamilyId == familyId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, now), ct);

    private (Guid Id, string Value, DateTime ExpiresAt) NewToken(DateTime now, DateTime absoluteExpiry)
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var value = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var expiry = now.AddDays(_settings.IdleDays);
        return (Guid.CreateVersion7(), value, expiry < absoluteExpiry ? expiry : absoluteExpiry);
    }

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
