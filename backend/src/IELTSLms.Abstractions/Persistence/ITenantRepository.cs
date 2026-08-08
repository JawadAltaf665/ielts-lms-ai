using IELTSLms.Domain.Tenancy;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>
/// Aggregate access for <see cref="Tenant"/>, plus the lookups tenant provisioning and resolution
/// need (Backend Architecture §5.5). The tenant is the isolation root, so these queries are not
/// tenant-scoped; uniqueness of slug/code is global.
/// </summary>
public interface ITenantRepository : IRepository<Tenant, TenantId>
{
    /// <summary>
    /// Loads a tenant by id <em>including</em> soft-deleted (closed) tenants, bypassing the soft-delete
    /// query filter. Required by restore, which by definition operates on a closed tenant.
    /// </summary>
    Task<Tenant?> GetByIdIncludingDeletedAsync(TenantId id, CancellationToken cancellationToken = default);

    /// <summary>Loads a tenant by its globally-unique slug, or <see langword="null"/> if none exists.</summary>
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a tenant by its custom primary domain (host-based resolution for branded domains,
    /// Multi-Tenant Architecture §5.1, §8), or <see langword="null"/> if none is mapped to it.
    /// </summary>
    Task<Tenant?> GetByPrimaryDomainAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>Loads a tenant by its globally-unique code, or <see langword="null"/> if none exists.</summary>
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Returns whether a tenant already exists with the given slug.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Returns whether a tenant already exists with the given code.</summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of tenants ordered by creation time (newest first) for administrative listing.
    /// Soft-deleted tenants are excluded by the global soft-delete filter.
    /// </summary>
    Task<IReadOnlyList<Tenant>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>Total number of (non-deleted) tenants, for pagination metadata.</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
