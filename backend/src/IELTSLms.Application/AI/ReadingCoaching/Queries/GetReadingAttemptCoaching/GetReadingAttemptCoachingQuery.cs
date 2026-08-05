using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestReadingCoaching;

namespace IELTSLms.Application.AI.ReadingCoaching.Queries.GetReadingAttemptCoaching;

/// <summary>
/// Explains *why* the wrong answers on a past <see cref="Domain.Reading.ReadingAttempt"/> were wrong,
/// grounded in the passage (Sprint 8). Thin: assembles the attempt's wrong answers plus the passage
/// text into grounding text and delegates to the existing
/// <see cref="RequestReadingCoachingCommand"/>/handler/job/prompt — no second, parallel gateway path
/// (mirrors <c>GetGrammarAttemptCoachingQuery</c>, Phase 7 Sprint 7). A <c>Query</c> in name (CQRS
/// layout) even though it has the same enqueue side effect as a command, because from the caller's
/// perspective this reads an attempt and returns an operation resource; the actual AI kickoff is
/// delegated, not duplicated.
/// </summary>
public sealed record GetReadingAttemptCoachingQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    // "analytics.view.cohort" (non-owner cohort access) is not implemented this sprint — only the
    // attempt's own owner can request coaching, same enforcement as GetReadingAttemptQuery.
    public string[] RequiredPermissions => ["reading.attempt"];
}
