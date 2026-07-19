using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Identity;

/// <summary>
/// An immutable audit record of a single authentication attempt (success or failure), used for
/// security review and anomaly signals (Auth §6). Append-only: once written it is never mutated.
/// </summary>
public sealed class LoginHistory : AggregateRoot<LoginHistoryId>
{
    private LoginHistory(
        LoginHistoryId id,
        UserId? userId,
        string emailAttempted,
        LoginOutcome outcome,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        UserId = userId;
        EmailAttempted = emailAttempted;
        Outcome = outcome;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OccurredAtUtc = occurredAtUtc;
    }

    private LoginHistory()
    {
        // EF Core materialization.
        EmailAttempted = null!;
    }

    /// <summary>The matched user, or <see langword="null"/> when the email matched no account.</summary>
    public UserId? UserId { get; private init; }

    /// <summary>The (normalized) email the attempt was made against.</summary>
    public string EmailAttempted { get; private init; }

    /// <summary>The outcome of the attempt.</summary>
    public LoginOutcome Outcome { get; private init; }

    /// <summary>IP address the attempt came from, if known.</summary>
    public string? IpAddress { get; private init; }

    /// <summary>User-agent string of the attempt, if known.</summary>
    public string? UserAgent { get; private init; }

    /// <summary>When the attempt occurred (UTC).</summary>
    public DateTimeOffset OccurredAtUtc { get; private init; }

    /// <summary>Records an authentication attempt.</summary>
    public static LoginHistory Record(
        LoginHistoryId id,
        UserId? userId,
        string emailAttempted,
        LoginOutcome outcome,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset occurredAtUtc) =>
        new(id, userId, emailAttempted, outcome, ipAddress, userAgent, occurredAtUtc);
}
