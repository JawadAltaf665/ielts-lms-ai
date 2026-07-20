using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// A device session: one login on one device, tracked so the user can view and revoke it
/// (Auth §4.2). Its id is surfaced as the <c>sid</c> access-token claim (Auth §9.1) and links the
/// refresh-token family. Sessions slide — each refresh extends <see cref="ExpiresAtUtc"/> up to
/// <see cref="AbsoluteExpiresAtUtc"/> (Auth §4.1).
/// </summary>
public sealed class UserSession : AggregateRoot<UserSessionId>, IAuditableEntity, ITenantScoped
{
    private UserSession(
        UserSessionId id,
        Guid tenantId,
        UserId userId,
        string? deviceLabel,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset absoluteExpiresAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        DeviceLabel = deviceLabel;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAtUtc = createdAtUtc;
        LastSeenAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        AbsoluteExpiresAtUtc = absoluteExpiresAtUtc;
    }

    private UserSession()
    {
        // EF Core materialization.
    }

    /// <summary>Owning tenant; scopes the session to its tenant for the global query filter (ERD §3).</summary>
    public Guid TenantId { get; private init; }

    /// <summary>The owning user.</summary>
    public UserId UserId { get; private init; }

    /// <summary>Human-friendly device label (e.g. browser/OS), if known.</summary>
    public string? DeviceLabel { get; private set; }

    /// <summary>IP address observed at session start / last refresh.</summary>
    public string? IpAddress { get; private set; }

    /// <summary>User-agent string observed for the session.</summary>
    public string? UserAgent { get; private set; }

    /// <summary>When the session was last active (UTC), updated on refresh.</summary>
    public DateTimeOffset LastSeenAtUtc { get; private set; }

    /// <summary>End of the current sliding window (UTC); extended on each refresh.</summary>
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    /// <summary>Hard cap on session lifetime (UTC); sliding never extends past this (Auth §4.1).</summary>
    public DateTimeOffset AbsoluteExpiresAtUtc { get; private init; }

    /// <summary>When the session was revoked (UTC), or <see langword="null"/> if active.</summary>
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>Why the session was revoked, for audit.</summary>
    public string? RevokedReason { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    /// <summary>Optimistic-concurrency token (xmin) maintained by PostgreSQL.</summary>
    public uint Version { get; private set; }

    /// <summary><see langword="true"/> once the session has been revoked.</summary>
    public bool IsRevoked => RevokedAtUtc is not null;

    /// <summary>Whether the session is usable: not revoked and within both expiry windows.</summary>
    public bool IsActiveAt(DateTimeOffset nowUtc) =>
        !IsRevoked && ExpiresAtUtc > nowUtc && AbsoluteExpiresAtUtc > nowUtc;

    /// <summary>Opens a new device session with a sliding and an absolute expiry window.</summary>
    public static UserSession Start(
        UserSessionId id,
        Guid tenantId,
        UserId userId,
        string? deviceLabel,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset nowUtc,
        TimeSpan slidingLifetime,
        TimeSpan absoluteLifetime)
    {
        if (slidingLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(slidingLifetime), "Sliding lifetime must be positive.");
        }

        if (absoluteLifetime < slidingLifetime)
        {
            throw new ArgumentOutOfRangeException(
                nameof(absoluteLifetime), "Absolute lifetime must be at least the sliding lifetime.");
        }

        return new UserSession(
            id,
            tenantId,
            userId,
            deviceLabel,
            ipAddress,
            userAgent,
            nowUtc,
            nowUtc.Add(slidingLifetime),
            nowUtc.Add(absoluteLifetime));
    }

    /// <summary>
    /// Extends the sliding window on a successful refresh, capped at the absolute expiry, and
    /// refreshes the last-seen metadata.
    /// </summary>
    public void SlideExpiry(DateTimeOffset nowUtc, TimeSpan slidingLifetime, string? ipAddress, string? userAgent)
    {
        var proposed = nowUtc.Add(slidingLifetime);
        ExpiresAtUtc = proposed > AbsoluteExpiresAtUtc ? AbsoluteExpiresAtUtc : proposed;
        LastSeenAtUtc = nowUtc;

        if (ipAddress is not null)
        {
            IpAddress = ipAddress;
        }

        if (userAgent is not null)
        {
            UserAgent = userAgent;
        }

        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Revokes the session with an audit reason. Idempotent; the first revocation wins.</summary>
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
