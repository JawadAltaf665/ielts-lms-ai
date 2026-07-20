using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A time-boxed, single-use token proving control of an email address (Auth §5). Only the hash of
/// the token is stored; the raw value is mailed to the user and never persisted.
/// </summary>
public sealed class EmailVerificationToken : AggregateRoot<EmailVerificationTokenId>, IAuditableEntity, ITenantScoped
{
    private EmailVerificationToken(
        EmailVerificationTokenId id,
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

    private EmailVerificationToken()
    {
        // EF Core materialization.
        TokenHash = null!;
    }

    /// <summary>Owning tenant; scopes the token to its tenant for the global query filter (ERD §3).</summary>
    public Guid TenantId { get; private init; }

    /// <summary>The user this token verifies.</summary>
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

    /// <summary>Issues a verification token for a user.</summary>
    public static EmailVerificationToken Issue(
        EmailVerificationTokenId id,
        Guid tenantId,
        UserId userId,
        string tokenHash,
        DateTimeOffset nowUtc,
        TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Verification-token lifetime must be positive.");
        }

        return new EmailVerificationToken(id, tenantId, userId, tokenHash, nowUtc, nowUtc.Add(lifetime));
    }

    /// <summary>Marks the token as redeemed so it cannot be used again.</summary>
    public void Consume(DateTimeOffset nowUtc)
    {
        ConsumedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
