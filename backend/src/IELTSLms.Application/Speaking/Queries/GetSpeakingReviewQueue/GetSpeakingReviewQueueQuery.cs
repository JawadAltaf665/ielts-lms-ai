using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingReviewQueue;

/// <summary>
/// Lists flagged/low-confidence <see cref="Domain.Speaking.SpeakingEvaluation"/>s awaiting human review
/// in the caller's tenant — the HITL consumer surface for the escalation policy checked in
/// <see cref="Domain.Speaking.SpeakingEvaluation.Create"/>. Cursor-paginated per API Design Standards
/// §6.1, newest first. There is no per-teacher cohort assignment concept in this codebase (grep-confirmed),
/// so "cohort-scoped to the requesting teacher" resolves to the same tenant + Teacher-role scoping every
/// other Speaking teacher-facing query already uses (see <c>GetSpeakingEvaluationQuery</c>).
/// </summary>
public sealed record GetSpeakingReviewQueueQuery(Guid TenantId, string? Cursor, int Limit)
    : IQuery<CursorPage<SpeakingReviewQueueItemResponse>>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.evaluate"];
}

public sealed record SpeakingReviewQueueItemResponse(
    Guid EvaluationId, Guid SpeakingAttemptId, decimal FluencyCoherence, decimal LexicalResource,
    decimal GrammaticalRangeAccuracy, decimal Pronunciation, decimal EstimatedBand, decimal Confidence,
    DateTimeOffset CreatedAtUtc);
