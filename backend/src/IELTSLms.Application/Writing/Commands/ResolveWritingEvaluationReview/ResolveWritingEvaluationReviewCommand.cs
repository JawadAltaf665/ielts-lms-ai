using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Queries.GetWritingEvaluation;

namespace IELTSLms.Application.Writing.Commands.ResolveWritingEvaluationReview;

/// <summary>
/// Records a teacher's review decision on a flagged evaluation (audited: actor, timestamp, decision) —
/// distinct from <c>PATCH /writing-evaluations/{id}</c> (<c>OverrideWritingEvaluationCommand</c>), which
/// changes scores. This records the decision even when no score changes; a teacher may also call the
/// override endpoint separately (before or after) if the scores themselves need correcting — either one
/// clears the evaluation from the review queue, see <see cref="Domain.Writing.WritingEvaluation.MarkReviewed"/>'s
/// remarks. Mirrors <c>ResolveSpeakingEvaluationReviewCommand</c> exactly, including its
/// teacher-only-checked-manually gap.
/// </summary>
public sealed record ResolveWritingEvaluationReviewCommand(Guid TenantId, Guid ResolvedBy, Guid EvaluationId, string Decision)
    : ICommand<WritingEvaluationResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["writing.evaluate"];
}
