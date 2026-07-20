using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A single-use, rotating refresh credential bound to a device session. The raw token is never
/// stored; only its hash is (Auth §3.1, §5). Tokens form a <em>family</em> (linked by
/// <see cref="SessionId"/>): rotation chains one token to the next, and presenting an already-used
/// token triggers family-wide revocation as a breach response (Auth §4.3).
/// </summary>
public sealed class RefreshToken : AggregateRoot<RefreshTokenId>, IAuditableEntity, ITenantScoped
{
    private RefreshToken(
        RefreshTokenId id,
        Guid tenantId,
        UserId userId,
        UserSessionId sessionId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        SessionId = sessionId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    private RefreshToken()
    {
        // EF Core materialization.
        TokenHash = null!;
    }

    /// <summary>Owning tenant; scopes the token to its tenant for the global query filter (ERD §3).</summary>
    public Guid TenantId { get; private init; }

    /// <summary>The owning user.</summary>
    public UserId UserId { get; private init; }

    /// <summary>The device session this token belongs to; the revocation-family key.</summary>
    public UserSessionId SessionId { get; private init; }

    /// <summary>SHA-256 hash (hex) of the opaque token value; the raw value is never persisted.</summary>
    public string TokenHash { get; private set; }

    /// <summary>When the token expires (UTC), after which it can no longer be refreshed.</summary>
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    /// <summary>When the token was consumed by rotation (UTC), or <see langword="null"/> if unused.</summary>
    public DateTimeOffset? UsedAtUtc { get; private set; }

    /// <summary>When the token was revoked (UTC), or <see langword="null"/> if still valid.</summary>
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>Why the token was revoked, for audit; <see langword="null"/> while active.</summary>
    public string? RevokedReason { get; private set; }

    /// <summary>The id of the token that replaced this one on rotation, if any.</summary>
    public RefreshTokenId? ReplacedByTokenId { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <summary><see langword="true"/> once the token has been consumed by a rotation.</summary>
    public bool IsUsed => UsedAtUtc is not null;

    /// <summary><see langword="true"/> once the token has been revoked.</summary>
    public bool IsRevoked => RevokedAtUtc is not null;

    /// <summary>Whether the token can still be exchanged: not used, not revoked, and not expired.</summary>
    public bool IsActiveAt(DateTimeOffset nowUtc) => !IsUsed && !IsRevoked && ExpiresAtUtc > nowUtc;

    /// <summary>Optimistic-concurrency token (xmin) maintained by PostgreSQL.</summary>
    public uint Version { get; private set; }

    /// <summary>Issues a new refresh token for a session.</summary>
    public static RefreshToken Issue(
        RefreshTokenId id,
        Guid tenantId,
        UserId userId,
        UserSessionId sessionId,
        string tokenHash,
        DateTimeOffset nowUtc,
        TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Refresh-token lifetime must be positive.");
        }

        return new RefreshToken(id, tenantId, userId, sessionId, tokenHash, nowUtc, nowUtc.Add(lifetime));
    }

    /// <summary>
    /// Marks this token as consumed by rotation, linking it to its replacement. Idempotency and
    /// reuse detection are the caller's responsibility (check <see cref="IsActiveAt"/> first).
    /// </summary>
    public void MarkRotated(RefreshTokenId replacedBy, DateTimeOffset nowUtc)
    {
        UsedAtUtc = nowUtc;
        ReplacedByTokenId = replacedBy;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Revokes the token with an audit reason. Safe to call repeatedly; the first wins.</summary>
    public void Revoke(string reason, DateTimeOffset nowUtc)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = nowUtc;
        RevokedReason = reason;
        UpdatedAtUtc = nowUtc;
    }
}
