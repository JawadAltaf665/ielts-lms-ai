using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing.Fakes;

internal sealed class FakeWritingEvaluationRepository : IWritingEvaluationRepository
{
    private readonly List<WritingEvaluation> _evaluations = [];

    public void Seed(WritingEvaluation evaluation) => _evaluations.Add(evaluation);

    public IReadOnlyList<WritingEvaluation> Evaluations => _evaluations;

    public Task<WritingEvaluation?> GetByIdAsync(WritingEvaluationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_evaluations.FirstOrDefault(e => e.Id == id));

    public void Add(WritingEvaluation aggregate) => _evaluations.Add(aggregate);

    public void Remove(WritingEvaluation aggregate) => _evaluations.Remove(aggregate);

    public Task<WritingEvaluation?> GetBySubmissionIdAsync(WritingSubmissionId submissionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_evaluations.FirstOrDefault(e => e.WritingSubmissionId == submissionId));

    public Task<IReadOnlyList<WritingEvaluation>> ListPendingReviewAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WritingEvaluation> pending = _evaluations
            .Where(e => e.TenantId == tenantId && e.RequiresHumanReview && e.ReviewedAtUtc is null && e.OverriddenAtUtc is null)
            .OrderByDescending(e => e.CreatedAtUtc).ThenByDescending(e => e.Id.Value)
            .ToList();
        return Task.FromResult(pending);
    }
}
