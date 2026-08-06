using IELTSLms.Domain.Learning;

namespace IELTSLms.Abstractions.Persistence.Learning;

/// <summary>Aggregate access for <see cref="LearningRecommendation"/> (Sprint 9).</summary>
public interface ILearningRecommendationRepository : IRepository<LearningRecommendation, LearningRecommendationId>
{
    Task<LearningRecommendation?> GetActiveByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>The most recently dismissed set, if any — feeds the "don't immediately re-recommend a
    /// just-dismissed item" business rule into the next generation's prompt context.</summary>
    Task<LearningRecommendation?> GetLatestDismissedAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
