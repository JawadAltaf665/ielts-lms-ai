using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Analytics.Queries.GetExamHistory;

/// <summary>
/// ponytail: loads each source's full per-user history into memory and merges/sorts/pages there,
/// rather than a single cross-table SQL UNION or a materialized read model — the simplest thing that
/// gives correct cursor semantics without a new read-model table. Fine while per-learner history stays
/// in the hundreds; move to a UNION view or projection table if that stops being true.
/// </summary>
public sealed class GetExamHistoryQueryHandler(
    IWritingSubmissionRepository writingSubmissions, ISpeakingAttemptRepository speakingAttempts,
    IReadingAttemptRepository readingAttempts, IListeningAttemptRepository listeningAttempts,
    IGrammarAttemptRepository grammarAttempts, IVocabularyProgressRepository vocabularyProgress,
    IMockExamResultRepository mockExamResults)
    : IQueryHandler<GetExamHistoryQuery, CursorPage<ExamHistoryItemResponse>>
{
    public async Task<Result<CursorPage<ExamHistoryItemResponse>>> Handle(
        GetExamHistoryQuery request, CancellationToken cancellationToken)
    {
        var all = new List<(DateTimeOffset AtUtc, Guid Id, ExamHistoryItemResponse Item)>();

        var writing = await writingSubmissions.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(writing.Select(s => (
            s.CreatedAtUtc, s.Id.Value, new ExamHistoryItemResponse("writing", s.Id.Value, s.CreatedAtUtc, s.Status.ToString(), null))));

        var speaking = await speakingAttempts.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(speaking.Select(a => (
            a.CreatedAtUtc, a.Id.Value, new ExamHistoryItemResponse("speaking", a.Id.Value, a.CreatedAtUtc, a.Status.ToString(), null))));

        var reading = await readingAttempts.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(reading.Select(a => (
            a.CreatedAtUtc, a.Id.Value,
            new ExamHistoryItemResponse("reading", a.Id.Value, a.CreatedAtUtc, a.IsSubmitted ? "Submitted" : "InProgress", a.Band))));

        var listening = await listeningAttempts.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(listening.Select(a => (
            a.CreatedAtUtc, a.Id.Value,
            new ExamHistoryItemResponse("listening", a.Id.Value, a.CreatedAtUtc, a.IsSubmitted ? "Submitted" : "InProgress", a.Band))));

        var grammar = await grammarAttempts.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(grammar.Select(a => (
            a.CreatedAtUtc, a.Id.Value, new ExamHistoryItemResponse("grammar", a.Id.Value, a.CreatedAtUtc, "Scored", null))));

        var vocabulary = await vocabularyProgress.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(vocabulary.Where(v => v.LastGrade is not null).Select(v => (
            v.UpdatedAtUtc ?? v.CreatedAtUtc, v.Id.Value,
            new ExamHistoryItemResponse("vocabulary", v.Id.Value, v.UpdatedAtUtc ?? v.CreatedAtUtc, "Reviewed", null))));

        var mockExams = await mockExamResults.ListByUserAsync(request.TenantId, request.UserId, cancellationToken).ConfigureAwait(false);
        all.AddRange(mockExams.Select(r => (
            r.CreatedAtUtc, r.Id.Value, new ExamHistoryItemResponse("mock-exam", r.Id.Value, r.CreatedAtUtc, "Finalized", r.OverallBand))));

        IEnumerable<(DateTimeOffset AtUtc, Guid Id, ExamHistoryItemResponse Item)> ordered =
            all.OrderByDescending(x => x.AtUtc).ThenByDescending(x => x.Id);

        if (ExamHistoryCursor.TryDecode(request.Cursor, out var cursorAtUtc, out var cursorId))
        {
            ordered = ordered.Where(x => x.AtUtc < cursorAtUtc || (x.AtUtc == cursorAtUtc && x.Id.CompareTo(cursorId) < 0));
        }

        var page = ordered.Take(request.Limit + 1).ToList();
        var hasMore = page.Count > request.Limit;
        var items = page.Take(request.Limit).ToList();

        var nextCursor = hasMore && items.Count > 0
            ? ExamHistoryCursor.Encode(items[^1].AtUtc, items[^1].Id)
            : null;

        return Result.Success(CursorPage<ExamHistoryItemResponse>.Create(items.Select(x => x.Item).ToArray(), nextCursor));
    }
}
