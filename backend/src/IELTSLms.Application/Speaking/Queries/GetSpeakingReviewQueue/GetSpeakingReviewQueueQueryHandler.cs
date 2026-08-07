using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingReviewQueue;

public sealed class GetSpeakingReviewQueueQueryHandler(ISpeakingEvaluationRepository evaluations, ICurrentUserService currentUser)
    : IQueryHandler<GetSpeakingReviewQueueQuery, CursorPage<SpeakingReviewQueueItemResponse>>
{
    private static readonly Error NotTeacher = Error.Forbidden(
        "AUTHZ_SPEAKING_REVIEW_QUEUE_TEACHER_ONLY", "Only a teacher may view the speaking review queue.");

    public async Task<Result<CursorPage<SpeakingReviewQueueItemResponse>>> Handle(
        GetSpeakingReviewQueueQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.Roles.Contains("Teacher"))
        {
            return Result.Failure<CursorPage<SpeakingReviewQueueItemResponse>>(NotTeacher);
        }

        var pending = await evaluations.ListPendingReviewAsync(request.TenantId, cancellationToken).ConfigureAwait(false);

        IEnumerable<SpeakingEvaluation> ordered = pending;
        if (SpeakingReviewCursor.TryDecode(request.Cursor, out var cursorAtUtc, out var cursorId))
        {
            ordered = ordered.Where(e =>
                e.CreatedAtUtc < cursorAtUtc || (e.CreatedAtUtc == cursorAtUtc && e.Id.Value.CompareTo(cursorId) < 0));
        }

        var page = ordered.Take(request.Limit + 1).ToList();
        var hasMore = page.Count > request.Limit;
        var items = page.Take(request.Limit).ToList();

        var nextCursor = hasMore && items.Count > 0
            ? SpeakingReviewCursor.Encode(items[^1].CreatedAtUtc, items[^1].Id.Value)
            : null;

        return Result.Success(CursorPage<SpeakingReviewQueueItemResponse>.Create(items.Select(ToResponse).ToArray(), nextCursor));
    }

    private static SpeakingReviewQueueItemResponse ToResponse(SpeakingEvaluation evaluation) => new(
        evaluation.Id.Value, evaluation.SpeakingAttemptId.Value, evaluation.FluencyCoherence, evaluation.LexicalResource,
        evaluation.GrammaticalRangeAccuracy, evaluation.Pronunciation, evaluation.EstimatedBand, evaluation.Confidence,
        evaluation.CreatedAtUtc);
}
