using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.Fakes;

internal sealed class FakeLearningRecommendationRepository : ILearningRecommendationRepository
{
    private readonly List<LearningRecommendation> _recommendations = [];

    public IReadOnlyList<LearningRecommendation> Recommendations => _recommendations;

    public void Seed(LearningRecommendation recommendation) => _recommendations.Add(recommendation);

    public Task<LearningRecommendation?> GetByIdAsync(LearningRecommendationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_recommendations.FirstOrDefault(r => r.Id == id));

    public void Add(LearningRecommendation aggregate) => _recommendations.Add(aggregate);

    public void Remove(LearningRecommendation aggregate) => _recommendations.Remove(aggregate);

    public Task<LearningRecommendation?> GetActiveByUserAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_recommendations.FirstOrDefault(
            r => r.TenantId == tenantId && r.UserId == userId && r.Status == LearningRecommendationStatus.Active));

    public Task<LearningRecommendation?> GetLatestDismissedAsync(
        Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(
            _recommendations
                .Where(r => r.TenantId == tenantId && r.UserId == userId && r.Status == LearningRecommendationStatus.Dismissed)
                .OrderByDescending(r => r.GeneratedAtUtc)
                .FirstOrDefault());
}
