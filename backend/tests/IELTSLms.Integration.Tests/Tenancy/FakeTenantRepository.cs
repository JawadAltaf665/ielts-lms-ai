using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Tenancy;

namespace IELTSLms.Integration.Tests.Tenancy;

/// <summary>Minimal in-memory <see cref="ITenantRepository"/> — only what HostTenantResolutionStrategy calls.</summary>
internal sealed class FakeTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _tenants = [];

    public void Seed(Tenant tenant) => _tenants.Add(tenant);

    public Task<Tenant?> GetByIdAsync(TenantId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Id == id));

    public void Add(Tenant aggregate) => _tenants.Add(aggregate);

    public void Remove(Tenant aggregate) => _tenants.Remove(aggregate);

    public Task<Tenant?> GetByIdIncludingDeletedAsync(TenantId id, CancellationToken cancellationToken = default) =>
        GetByIdAsync(id, cancellationToken);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Slug.Value == slug));

    public Task<Tenant?> GetByPrimaryDomainAsync(string domain, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.PrimaryDomain == domain));

    public Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Code.Value == code));

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenants.Any(t => t.Slug.Value == slug));

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tenants.Any(t => t.Code.Value == code));

    public Task<IReadOnlyList<Tenant>> ListAsync(int skip, int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Tenant>>(_tenants.Skip(skip).Take(take).ToList());

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(_tenants.Count);
}
