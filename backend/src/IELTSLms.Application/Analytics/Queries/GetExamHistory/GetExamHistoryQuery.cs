using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Common.Pagination;

namespace IELTSLms.Application.Analytics.Queries.GetExamHistory;

/// <summary>
/// Cross-skill attempt/submission history (sprint API table), cursor-paginated per API Design
/// Standards §6.1. Aggregates <c>WritingSubmissions</c>, <c>SpeakingAttempts</c>, <c>ReadingAttempts</c>,
/// <c>ListeningAttempts</c>, <c>GrammarAttempts</c>, <c>VocabularyProgress</c>, and <c>MockExamResults</c>
/// — read-only, no new write path.
/// </summary>
public sealed record GetExamHistoryQuery(Guid TenantId, Guid UserId, string? Cursor, int Limit) : IQuery<CursorPage<ExamHistoryItemResponse>>;

/// <summary><see cref="Type"/> matches the review endpoint's <c>{type}</c> route segment
/// (writing|speaking|reading|listening|grammar|vocabulary|mock-exam), so a client can build the review
/// link directly from a history item.</summary>
public sealed record ExamHistoryItemResponse(string Type, Guid Id, DateTimeOffset AtUtc, string Status, decimal? Band);
