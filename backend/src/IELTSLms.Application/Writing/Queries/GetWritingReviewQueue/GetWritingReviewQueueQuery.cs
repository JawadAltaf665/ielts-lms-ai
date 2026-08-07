using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.Writing.Queries.GetWritingReviewQueue;

/// <summary>
/// Lists flagged/low-confidence/high-similarity <see cref="Domain.Writing.WritingEvaluation"/>s awaiting
/// human review in the caller's tenant — the HITL consumer surface for the escalation policy checked in
/// <see cref="Domain.Writing.WritingEvaluation.Create"/>. Cursor-paginated per API Design Standards §6.1,
/// newest first. Mirrors <c>GetSpeakingReviewQueueQuery</c> exactly.
/// </summary>
public sealed record GetWritingReviewQueueQuery(Guid TenantId, string? Cursor, int Limit)
    : IQuery<CursorPage<WritingReviewQueueItemResponse>>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.evaluate"];
}

public sealed record WritingReviewQueueItemResponse(
    Guid EvaluationId, Guid WritingSubmissionId, decimal TaskResponse, decimal CoherenceCohesion, decimal LexicalResource,
    decimal GrammaticalRangeAccuracy, decimal EstimatedBand, decimal Confidence, decimal? SimilarityScore,
    DateTimeOffset CreatedAtUtc);
