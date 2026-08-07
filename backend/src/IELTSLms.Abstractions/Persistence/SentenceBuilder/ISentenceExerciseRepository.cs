using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Abstractions.Persistence.SentenceBuilder;

/// <summary>
/// Aggregate access for <see cref="SentenceExercise"/> (ERD ENT-019). Not tenant-scoped by the
/// automatic global filter (<see cref="SentenceExercise.TenantId"/> is nullable), so every read here
/// explicitly includes global (<c>TenantId == null</c>) exercises alongside the caller's own
/// tenant-authored ones.
/// </summary>
public interface ISentenceExerciseRepository : IRepository<SentenceExercise, SentenceExerciseId>
{
    /// <summary>The exercise if it is global or owned by <paramref name="tenantId"/>; otherwise <see langword="null"/> (404, not 403).</summary>
    Task<SentenceExercise?> GetVisibleByIdAsync(Guid tenantId, SentenceExerciseId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SentenceExercise>> ListVisibleAsync(
        Guid tenantId, SentenceDifficulty? difficulty, int skip, int take, CancellationToken cancellationToken = default);

    Task<long> CountVisibleAsync(Guid tenantId, SentenceDifficulty? difficulty, CancellationToken cancellationToken = default);
}
