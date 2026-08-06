using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.Fakes;

internal sealed class FakeWeaknessProfileRepository : IWeaknessProfileRepository
{
    private readonly List<WeaknessProfile> _profiles = [];

    public IReadOnlyList<WeaknessProfile> Profiles => _profiles;

    public void Seed(WeaknessProfile profile) => _profiles.Add(profile);

    public Task<WeaknessProfile?> GetByIdAsync(WeaknessProfileId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_profiles.FirstOrDefault(p => p.Id == id));

    public void Add(WeaknessProfile aggregate) => _profiles.Add(aggregate);

    public void Remove(WeaknessProfile aggregate) => _profiles.Remove(aggregate);

    public Task<WeaknessProfile?> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_profiles.FirstOrDefault(p => p.TenantId == tenantId && p.UserId == userId));
}
