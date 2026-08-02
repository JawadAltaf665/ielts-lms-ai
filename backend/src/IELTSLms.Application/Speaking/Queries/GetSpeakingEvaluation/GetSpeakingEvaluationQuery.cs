using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;

/// <summary>Gets one evaluation detail — own attempt's evaluation, or (Teacher role) any in the tenant.
/// See <see cref="Queries.GetSpeakingAttempt.GetSpeakingAttemptQuery"/> for the ownership-check shape.</summary>
public sealed record GetSpeakingEvaluationQuery(Guid TenantId, Guid UserId, Guid EvaluationId)
    : IQuery<SpeakingEvaluationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.submit"];
}

public sealed record SpeakingEvaluationResponse(
    Guid Id, Guid SpeakingAttemptId, decimal FluencyCoherence, decimal LexicalResource, decimal GrammaticalRangeAccuracy,
    decimal Pronunciation, decimal EstimatedBand, decimal Confidence, string FeedbackText, bool RequiresHumanReview,
    bool IsOverridden, decimal? OverriddenFluencyCoherence, decimal? OverriddenLexicalResource,
    decimal? OverriddenGrammaticalRangeAccuracy, decimal? OverriddenPronunciation, decimal? OverriddenBand, Guid? OverriddenBy,
    DateTimeOffset? OverriddenAtUtc, string? OverrideReason, bool IsReviewed, Guid? ReviewedBy, DateTimeOffset? ReviewedAtUtc,
    string? ReviewDecision);
