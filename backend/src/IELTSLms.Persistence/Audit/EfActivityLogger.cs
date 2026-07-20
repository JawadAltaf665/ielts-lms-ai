using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Serialization;
using IELTSLms.Abstractions.Time;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Domain.Audit;

namespace IELTSLms.Persistence.Audit;

internal sealed class EfActivityLogger(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    ITenantContext tenantContext,
    ICorrelationIdProvider correlationIdProvider,
    IRequestContext requestContext,
    IJsonSerializer jsonSerializer,
    IClock clock) : IActivityLogger
{
    public async Task LogAsync(
        string activityType,
        Guid? userId,
        Guid? tenantId,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default)
    {
        var log = ActivityLog.Create(
            activityType,
            userId ?? currentUserService.UserId,
            tenantId ?? tenantContext.TenantId,
            clock.UtcNow,
            metadata is { Count: > 0 } ? jsonSerializer.Serialize(metadata) : null,
            requestContext.ClientIp,
            requestContext.UserAgent,
            correlationIdProvider.CorrelationId);

        dbContext.Set<ActivityLog>().Add(log);
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
