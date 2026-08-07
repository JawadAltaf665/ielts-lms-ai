using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;

namespace IELTSLms.Application.AI.GrammarCoaching.Queries.GetGrammarAttemptCoaching;

/// <summary>
/// Explains *why* the wrong answers on a past <see cref="Domain.Grammar.GrammarAttempt"/> were wrong
/// (Sprint 7, wiring Grammar Coach — Phase 6 Sprint 3 — into learner-facing review). Thin: assembles
/// the attempt's wrong answers into text and delegates to the existing
/// <see cref="RequestGrammarCoachingCommand"/>/handler/job/prompt — no second, parallel gateway path.
/// A <c>Query</c> in name (CQRS layout) even though it has the same enqueue side effect as a command,
/// because from the caller's perspective this reads an attempt and returns an operation resource; the
/// actual AI kickoff is delegated, not duplicated.
/// </summary>
public sealed record GetGrammarAttemptCoachingQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    // "analytics.view.cohort" (non-owner cohort access) is not implemented this sprint — only the
    // attempt's own owner can request coaching, same enforcement as GetGrammarAttemptQuery.
    public string[] RequiredPermissions => ["grammar.attempt"];
}
