using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Writing.Queries.GetWritingEvaluation;

/// <summary>Gets one evaluation detail — own submission's evaluation, or (Teacher role) any in the
/// tenant. See <see cref="IELTSLms.Application.Writing.Queries.GetWritingSubmission.GetWritingSubmissionQuery"/>
/// for the ownership-check shape.</summary>
public sealed record GetWritingEvaluationQuery(Guid TenantId, Guid UserId, Guid EvaluationId)
    : IQuery<WritingEvaluationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.submit"];
}

public sealed record WritingEvaluationResponse(
    Guid Id, Guid WritingSubmissionId, decimal TaskResponse, decimal CoherenceCohesion, decimal LexicalResource,
    decimal GrammaticalRangeAccuracy, decimal EstimatedBand, decimal Confidence, string FeedbackText, decimal? SimilarityScore,
    bool RequiresHumanReview, bool IsOverridden, decimal? OverriddenTaskResponse, decimal? OverriddenCoherenceCohesion,
    decimal? OverriddenLexicalResource, decimal? OverriddenGrammaticalRangeAccuracy, decimal? OverriddenBand, Guid? OverriddenBy,
    DateTimeOffset? OverriddenAtUtc, string? OverrideReason, bool IsReviewed, Guid? ReviewedBy, DateTimeOffset? ReviewedAtUtc,
    string? ReviewDecision);
