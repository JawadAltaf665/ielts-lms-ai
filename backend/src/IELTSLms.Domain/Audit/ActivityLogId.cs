namespace IELTSLms.Domain.Audit;

public readonly record struct ActivityLogId(Guid Value)
{
    public static ActivityLogId New() => new(Guid.CreateVersion7());
}
