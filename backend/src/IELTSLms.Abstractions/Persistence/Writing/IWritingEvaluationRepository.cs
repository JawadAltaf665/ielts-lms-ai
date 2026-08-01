using IELTSLms.Domain.Writing;

namespace IELTSLms.Abstractions.Persistence.Writing;

/// <summary>Aggregate access for <see cref="WritingEvaluation"/> (ERD ENT-029).</summary>
public interface IWritingEvaluationRepository : IRepository<WritingEvaluation, WritingEvaluationId>
{
    /// <summary>The single active evaluation for a submission, or <see langword="null"/> if not yet evaluated (ERD's 1:1 constraint).</summary>
    Task<WritingEvaluation?> GetBySubmissionIdAsync(WritingSubmissionId submissionId, CancellationToken cancellationToken = default);
}
