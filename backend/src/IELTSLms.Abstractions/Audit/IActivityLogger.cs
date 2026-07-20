namespace IELTSLms.Abstractions.Audit;

public interface IActivityLogger
{
    Task LogAsync(
        string activityType,
        Guid? userId,
        Guid? tenantId,
        Dictionary<string, object>? metadata = null,
        CancellationToken ct = default);
}
