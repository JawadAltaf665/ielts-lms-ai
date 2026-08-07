using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking.Fakes;

internal sealed class FakeSpeakingEvaluationRepository : ISpeakingEvaluationRepository
{
    private readonly List<SpeakingEvaluation> _evaluations = [];

    public void Seed(SpeakingEvaluation evaluation) => _evaluations.Add(evaluation);

    public IReadOnlyList<SpeakingEvaluation> Evaluations => _evaluations;

    public Task<SpeakingEvaluation?> GetByIdAsync(SpeakingEvaluationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_evaluations.FirstOrDefault(e => e.Id == id));

    public void Add(SpeakingEvaluation aggregate) => _evaluations.Add(aggregate);

    public void Remove(SpeakingEvaluation aggregate) => _evaluations.Remove(aggregate);

    public Task<SpeakingEvaluation?> GetByAttemptIdAsync(SpeakingAttemptId attemptId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_evaluations.FirstOrDefault(e => e.SpeakingAttemptId == attemptId));

    public Task<IReadOnlyList<SpeakingEvaluation>> ListPendingReviewAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SpeakingEvaluation> pending = _evaluations
            .Where(e => e.TenantId == tenantId && e.RequiresHumanReview && e.ReviewedAtUtc is null && e.OverriddenAtUtc is null)
            .OrderByDescending(e => e.CreatedAtUtc).ThenByDescending(e => e.Id.Value)
            .ToList();
        return Task.FromResult(pending);
    }
}
