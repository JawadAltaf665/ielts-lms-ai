using IELTSLms.SharedKernel.Domain;

namespace IELTSLms.Domain.Audit;

/// <summary>
/// Immutable append-only record of user/system activity (login, security events, admin actions).
/// </summary>
public sealed class ActivityLog : Entity<ActivityLogId>
{
    private ActivityLog() { }

    public static ActivityLog Create(
        string activityType,
        Guid? userId,
        Guid? tenantId,
        DateTimeOffset timestamp,
        string? metadata,
        string? ipAddress,
        string? userAgent,
        string? correlationId)
    {
        return new ActivityLog
        {
            Id = ActivityLogId.New(),
            ActivityType = activityType,
            UserId = userId,
            TenantId = tenantId,
            Timestamp = timestamp,
            Metadata = metadata,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CorrelationId = correlationId,
        };
    }

    public string ActivityType { get; private init; } = default!;
    public Guid? UserId { get; private init; }
    public Guid? TenantId { get; private init; }
    public DateTimeOffset Timestamp { get; private init; }
    public string? Metadata { get; private init; }
    public string? IpAddress { get; private init; }
    public string? UserAgent { get; private init; }
    public string? CorrelationId { get; private init; }
}
