using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.SentenceBuilder.Commands.SubmitSentenceAttempt;

namespace IELTSLms.Application.SentenceBuilder.Queries.GetSentenceAttempt;

/// <summary>Gets a past attempt and its result, including AI scaffolding feedback once the async
/// enrichment has completed (<see cref="SentenceAttemptResponse.AiResponseId"/>/<c>Feedback</c> are
/// <see langword="null"/> until then) — client polls this resource, same pattern as Grammar's
/// synchronous grading with optional later enrichment.</summary>
public sealed record GetSentenceAttemptQuery(Guid TenantId, Guid UserId, Guid AttemptId)
    : IQuery<SentenceAttemptResponse>, IAuthorizeRequest
{
    // "analytics.view.cohort" (non-owner cohort access) is not implemented this sprint — only the
    // attempt's own owner can view it, same enforcement as GetGrammarAttemptQuery.
    public string[] RequiredPermissions => ["sentence-builder.attempt"];
}
