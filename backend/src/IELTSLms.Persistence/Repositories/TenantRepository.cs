using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace IELTSLms.Persistence.Repositories;

/// <summary>
/// EF Core <see cref="ITenantRepository"/>: aggregate access plus the global (non-tenant-scoped)
/// slug/code lookups and administrative listing used by tenant provisioning and resolution.
/// </summary>
internal sealed class TenantRepository(ApplicationDbContext dbContext)
    : RepositoryBase<Tenant, TenantId>(dbContext), ITenantRepository
{
    /// <inheritdoc />
    public async Task<Tenant?> GetByIdIncludingDeletedAsync(TenantId id, CancellationToken cancellationToken = default) =>
        await Set.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.Slug == TenantSlug.FromTrusted(slug), cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Tenant?> GetByPrimaryDomainAsync(string domain, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.PrimaryDomain == domain, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await Set.FirstOrDefaultAsync(t => t.Code == TenantCode.FromTrusted(code), cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(t => t.Slug == TenantSlug.FromTrusted(slug), cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        await Set.AnyAsync(t => t.Code == TenantCode.FromTrusted(code), cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Tenant>> ListAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        await Set.OrderByDescending(t => t.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        await Set.CountAsync(cancellationToken).ConfigureAwait(false);
}
