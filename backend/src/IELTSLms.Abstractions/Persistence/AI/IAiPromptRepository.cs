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

    /// <summary>
    /// Same resolution order as <see cref="GetActiveForFeatureAsync"/> (tenant override, else global
    /// default) but additionally scoped — for a feature with more than one live prompt "slot" at once,
    /// e.g. AI Teacher's one prompt per <c>LessonStep</c> under the single <c>ai-teacher</c> feature tag
    /// (Phase 7 Sprint 4). <see cref="GetActiveForFeatureAsync"/> is left untouched for the single-slot
    /// capabilities that already call it (Grammar Coach, conversation summarization, AI Tutor).
    /// </summary>
    Task<AiPrompt?> GetActiveForFeatureAndScopeAsync(
        Guid tenantId, string feature, string scope, CancellationToken cancellationToken = default);

    /// <summary>
    /// All versions in one tenant-scoped (Feature, Scope) slot, newest first — the admin/versioning
    /// surface's single query for computing the next version number, finding whichever version is
    /// currently active in the slot, and finding the prior version to restore on rollback.
    /// </summary>
    Task<IReadOnlyList<AiPrompt>> GetVersionsAsync(
        Guid tenantId, string feature, string scope, CancellationToken cancellationToken = default);

    /// <summary>Tenant-scoped, filterable browse of prompt templates for the admin management surface.</summary>
    Task<IReadOnlyList<AiPrompt>> ListAsync(
        Guid tenantId, string? feature, string? scope, bool? isActive, int skip, int take,
        CancellationToken cancellationToken = default);

    /// <summary>Total count matching the same filters as <see cref="ListAsync"/>, for pagination.</summary>
    Task<int> CountAsync(
        Guid tenantId, string? feature, string? scope, bool? isActive, CancellationToken cancellationToken = default);
}
