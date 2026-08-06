using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Assessment.Queries.GetOrchestratedReview;

/// <summary>
/// Retrieves the status and step trace of a Monthly Mock Exam Workflow run (Sprint 10).
/// Polled by the client after finalizing a mock exam to observe the asynchronous orchestrated review.
/// </summary>
public sealed record GetOrchestratedReviewQuery(Guid TenantId, Guid UserId, Guid MockExamId) : IQuery<OrchestratedReviewResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["mock-exam.read"];
}

public sealed record OrchestratedReviewResponse(
    Guid WorkflowRunId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? StepTraceJson);
