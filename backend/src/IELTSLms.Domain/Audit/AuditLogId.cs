namespace IELTSLms.Domain.Audit;

public readonly record struct AuditLogId(Guid Value)
{
    public static AuditLogId New() => new(Guid.CreateVersion7());
}
