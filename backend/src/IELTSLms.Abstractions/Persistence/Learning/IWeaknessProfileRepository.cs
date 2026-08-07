using IELTSLms.Domain.Learning;

namespace IELTSLms.Abstractions.Persistence.Learning;

/// <summary>Aggregate access for <see cref="WeaknessProfile"/> (Sprint 9) — one row per learner, upserted.</summary>
public interface IWeaknessProfileRepository : IRepository<WeaknessProfile, WeaknessProfileId>
{
    Task<WeaknessProfile?> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
