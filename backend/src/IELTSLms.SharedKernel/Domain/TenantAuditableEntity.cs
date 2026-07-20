namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Standard base for tenant-scoped aggregate roots: full audit + tenant isolation + concurrency.
/// This is the "batteries-included" base that new domain entities should derive from.
/// </summary>
public abstract class TenantAuditableEntity<TId> : AuditableEntity<TId>, ITenantScoped
    where TId : notnull
{
    protected TenantAuditableEntity(TId id) : base(id)
    {
    }

    protected TenantAuditableEntity()
    {
    }

    public Guid TenantId { get; protected set; }
}
