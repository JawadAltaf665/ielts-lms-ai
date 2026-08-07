using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;

namespace IELTSLms.Application.Speaking.Commands.ResolveSpeakingEvaluationReview;

/// <summary>
/// Records a teacher's review decision on a flagged evaluation (audited: actor, timestamp, decision) —
/// distinct from <c>PATCH /speaking-evaluations/{id}</c> (<c>OverrideSpeakingEvaluationCommand</c>),
/// which changes scores. This records the decision even when no score changes; a teacher may also call
/// the override endpoint separately (before or after) if the scores themselves need correcting — either
/// one clears the evaluation from the review queue, see
/// <see cref="Domain.Speaking.SpeakingEvaluation.MarkReviewed"/>'s remarks. Teacher-only, checked
/// manually in the handler, same established gap as every other fine-grained permission in this codebase
/// (mirrors <c>OverrideSpeakingEvaluationCommand</c>).
/// </summary>
public sealed record ResolveSpeakingEvaluationReviewCommand(Guid TenantId, Guid ResolvedBy, Guid EvaluationId, string Decision)
    : ICommand<SpeakingEvaluationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.evaluate"];
}
