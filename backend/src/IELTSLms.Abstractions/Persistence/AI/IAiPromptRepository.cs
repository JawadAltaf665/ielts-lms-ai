using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>
/// Aggregate access for <see cref="AiPrompt"/> (ERD ENT-051). Not tenant-scoped by the automatic
/// global filter (<see cref="AiPrompt.TenantId"/> is nullable) — same exemption as
/// <c>IGrammarTopicRepository</c>.
/// </summary>
public interface IAiPromptRepository : IRepository<AiPrompt, AiPromptId>
{
    /// <summary>
    /// Resolves the active prompt for a feature: a tenant-specific override if one exists, otherwise
    /// the global default (<c>TenantId == null</c>) — mirrors <c>TenantFeature</c>'s resolution order.
    /// </summary>
    Task<AiPrompt?> GetActiveForFeatureAsync(Guid tenantId, string feature, CancellationToken cancellationToken = default);
}
