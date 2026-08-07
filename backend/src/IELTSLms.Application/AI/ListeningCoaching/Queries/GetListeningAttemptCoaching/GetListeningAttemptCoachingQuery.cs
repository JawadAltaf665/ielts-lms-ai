using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestListeningCoaching;

namespace IELTSLms.Application.AI.ListeningCoaching.Queries.GetListeningAttemptCoaching;

/// <summary>
/// Explains *why* the wrong answers on a past <see cref="Domain.Listening.ListeningAttempt"/> were
/// wrong, grounded in the audio's transcript (Sprint 8). Mirrors
/// <c>GetReadingAttemptCoachingQuery</c> exactly.
/// </summary>
public sealed record GetListeningAttemptCoachingQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<AiOperationAcceptedResponse>, IAuthorizeRequest
{
    // "analytics.view.cohort" (non-owner cohort access) is not implemented this sprint — only the
    // attempt's own owner can request coaching, same enforcement as GetListeningAttemptQuery.
    public string[] RequiredPermissions => ["listening.attempt"];
}
