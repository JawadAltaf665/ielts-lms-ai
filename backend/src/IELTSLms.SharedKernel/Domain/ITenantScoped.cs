namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marks an entity as belonging to exactly one tenant. The persistence layer applies a mandatory
/// global query filter on <see cref="TenantId"/> to every <see cref="ITenantScoped"/> entity, so
/// no query can accidentally cross the tenant boundary (Multi-Tenant Architecture §6, ERD §3). The
/// tenant root itself (<c>Tenant</c>) is deliberately <em>not</em> tenant-scoped.
/// </summary>
public interface ITenantScoped
{
    /// <summary>The owning tenant. Set once at creation and never changed.</summary>
    Guid TenantId { get; }
}
