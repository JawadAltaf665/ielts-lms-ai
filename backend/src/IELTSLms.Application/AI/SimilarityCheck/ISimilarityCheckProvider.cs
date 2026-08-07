using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.AI.SimilarityCheck;

/// <summary>
/// The plagiarism/similarity-check tool named by Agentic AI Architecture §12.2's Writing Examiner Agent
/// row (Phase 7 Sprint 6). First-pass implementation per the sprint's own ponytail-lazy guidance:
/// within-tenant corpus comparison against the tenant's own historical <see cref="WritingSubmission"/>s
/// for the same task — cheap, self-hosted, catches copy-between-students within a cohort — rather than a
/// hosted external similarity API. Runs in parallel with the Grammar/Vocabulary agents in
/// <c>WritingEvaluationJob</c>, never sequentially, and its result only ever flags
/// (<c>WritingEvaluation.RequiresHumanReview</c>), never blocks scoring or publication.
/// </summary>
public interface ISimilarityCheckProvider
{
    Task<SimilarityCheckResult> CheckAsync(
        Guid tenantId, WritingTaskId taskId, WritingSubmissionId submissionId, string responseText,
        CancellationToken cancellationToken = default);
}

/// <summary><paramref name="Score"/> is 0 when there is no comparable prior submission for this task in the tenant.</summary>
public sealed record SimilarityCheckResult(decimal Score, Guid? MatchedSubmissionId);
