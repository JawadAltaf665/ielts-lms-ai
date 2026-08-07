using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Queries.GetWritingReviewQueue;

public sealed class GetWritingReviewQueueQueryHandler(IWritingEvaluationRepository evaluations, ICurrentUserService currentUser)
    : IQueryHandler<GetWritingReviewQueueQuery, CursorPage<WritingReviewQueueItemResponse>>
{
    private static readonly Error NotTeacher = Error.Forbidden(
        "AUTHZ_WRITING_REVIEW_QUEUE_TEACHER_ONLY", "Only a teacher may view the writing review queue.");

    public async Task<Result<CursorPage<WritingReviewQueueItemResponse>>> Handle(
        GetWritingReviewQueueQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.Roles.Contains("Teacher"))
        {
            return Result.Failure<CursorPage<WritingReviewQueueItemResponse>>(NotTeacher);
        }

        var pending = await evaluations.ListPendingReviewAsync(request.TenantId, cancellationToken).ConfigureAwait(false);

        IEnumerable<WritingEvaluation> ordered = pending;
        if (WritingReviewCursor.TryDecode(request.Cursor, out var cursorAtUtc, out var cursorId))
        {
            ordered = ordered.Where(e =>
                e.CreatedAtUtc < cursorAtUtc || (e.CreatedAtUtc == cursorAtUtc && e.Id.Value.CompareTo(cursorId) < 0));
        }

        var page = ordered.Take(request.Limit + 1).ToList();
        var hasMore = page.Count > request.Limit;
        var items = page.Take(request.Limit).ToList();

        var nextCursor = hasMore && items.Count > 0
            ? WritingReviewCursor.Encode(items[^1].CreatedAtUtc, items[^1].Id.Value)
            : null;

        return Result.Success(CursorPage<WritingReviewQueueItemResponse>.Create(items.Select(ToResponse).ToArray(), nextCursor));
    }

    private static WritingReviewQueueItemResponse ToResponse(WritingEvaluation evaluation) => new(
        evaluation.Id.Value, evaluation.WritingSubmissionId.Value, evaluation.TaskResponse, evaluation.CoherenceCohesion,
        evaluation.LexicalResource, evaluation.GrammaticalRangeAccuracy, evaluation.EstimatedBand, evaluation.Confidence,
        evaluation.SimilarityScore, evaluation.CreatedAtUtc);
}
