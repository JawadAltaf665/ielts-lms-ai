using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A time-boxed, single-use token authorizing a password reset (Auth §5). Only the hash is stored;
/// the raw value is mailed to the user. Redeeming it revokes existing sessions (handled by the
/// application/domain on the <see cref="User"/> aggregate).
/// </summary>
public sealed class PasswordResetToken : AggregateRoot<PasswordResetTokenId>, IAuditableEntity, ITenantScoped
{
    private PasswordResetToken(
        PasswordResetTokenId id,
        Guid tenantId,
        UserId userId,
        string tokenHash,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    private PasswordResetToken()
    {
        // EF Core materialization.
        TokenHash = null!;
    }

    /// <summary>Owning tenant; scopes the token to its tenant for the global query filter (ERD §3).</summary>
    public Guid TenantId { get; private init; }

    /// <summary>The user this token can reset.</summary>
    public UserId UserId { get; private init; }

    /// <summary>SHA-256 hash (hex) of the opaque token value.</summary>
    public string TokenHash { get; private init; }

    /// <summary>When the token expires (UTC).</summary>
    public DateTimeOffset ExpiresAtUtc { get; private init; }

    /// <summary>When the token was redeemed (UTC), or <see langword="null"/> if unused.</summary>
    public DateTimeOffset? ConsumedAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <summary>Optimistic-concurrency token (xmin) maintained by PostgreSQL.</summary>
    public uint Version { get; private set; }

    /// <summary><see langword="true"/> once the token has been consumed.</summary>
    public bool IsConsumed => ConsumedAtUtc is not null;

    /// <summary>Whether the token can still be redeemed: not consumed and not expired.</summary>
    public bool IsRedeemableAt(DateTimeOffset nowUtc) => !IsConsumed && ExpiresAtUtc > nowUtc;

    /// <summary>Issues a reset token for a user.</summary>
    public static PasswordResetToken Issue(
        PasswordResetTokenId id,
        Guid tenantId,
        UserId userId,
        string tokenHash,
        DateTimeOffset nowUtc,
        TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Reset-token lifetime must be positive.");
        }

        return new PasswordResetToken(id, tenantId, userId, tokenHash, nowUtc, nowUtc.Add(lifetime));
    }

    /// <summary>Marks the token as redeemed so it cannot be used again.</summary>
    public void Consume(DateTimeOffset nowUtc)
    {
        ConsumedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
