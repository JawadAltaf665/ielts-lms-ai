namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Abstract base for aggregate roots that carry full audit, soft-delete, restore, and concurrency.
/// Future domain entities should extend this as their standard base.
/// </summary>
public abstract class AuditableEntity<TId> : AggregateRoot<TId>,
    IFullyAuditable, IConcurrencyAware
    where TId : notnull
{
    protected AuditableEntity(TId id) : base(id)
    {
    }

    protected AuditableEntity()
    {
    }

    public DateTimeOffset CreatedAtUtc { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public DateTimeOffset? UpdatedAtUtc { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletedAtUtc { get; protected set; }
    public Guid? DeletedBy { get; protected set; }
    public DateTimeOffset? RestoredAtUtc { get; protected set; }
    public Guid? RestoredBy { get; protected set; }
    public uint Version { get; protected set; }
}
