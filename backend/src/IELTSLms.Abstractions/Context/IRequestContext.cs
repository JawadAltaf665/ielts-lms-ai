namespace IELTSLms.Abstractions.Context;

/// <summary>
/// Immutable request context populated at the API boundary and available to all layers.
/// </summary>
public interface IRequestContext
{
    string CorrelationId { get; }
    string RequestId { get; }
    string? TraceId { get; }
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? ClientIp { get; }
    string? UserAgent { get; }
    DateTimeOffset RequestStartTime { get; }
}
