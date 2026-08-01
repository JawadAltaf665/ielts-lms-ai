using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>
/// Aggregate access for <see cref="AiProvider"/> (ERD ENT-050). Not tenant-scoped by the automatic
/// global filter (<see cref="AiProvider.TenantId"/> is nullable) — same exemption as
/// <c>IGrammarTopicRepository</c>.
/// </summary>
public interface IAiProviderRepository : IRepository<AiProvider, AiProviderId>
{
    /// <summary>The active global-catalog provider for the given vendor, or <see langword="null"/> if not seeded.</summary>
    Task<AiProvider?> GetActiveByKindAsync(AiProviderKind kind, CancellationToken cancellationToken = default);
}
