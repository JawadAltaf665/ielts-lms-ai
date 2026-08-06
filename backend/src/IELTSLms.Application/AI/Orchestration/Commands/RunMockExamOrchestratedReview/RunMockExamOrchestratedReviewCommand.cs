using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Orchestration.Commands.RunMockExamOrchestratedReview;

/// <summary>
/// Triggers the Monthly Mock Exam Workflow for a completed mock exam (Sprint 10, Workflow Engine).
/// Called by <see cref="EventHandlers.MockExamCompletedDomainEventHandler"/> — not exposed as a
/// direct API endpoint (the client polls <c>GET /api/v1/mock-exams/{id}/orchestrated-review</c>).
///
/// The workflow orchestrates: ReadingScore + ListeningScore (parallel), BandPredictor, and
/// LearningRecommendation, using scores already computed by the section evaluation jobs.
/// The <see cref="WorkflowRun"/> is created in Running state and persisted immediately so the
/// polling endpoint can return a pending status while work is in progress.
/// </summary>
public sealed record RunMockExamOrchestratedReviewCommand(
    Guid TenantId,
    Guid UserId,
    Guid MockExamId,
    decimal OverallBand)
    : ICommand<WorkflowRunId>;
