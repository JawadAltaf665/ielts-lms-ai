using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;

public sealed class GetSpeakingEvaluationQueryHandler(
    ISpeakingEvaluationRepository evaluations, ISpeakingAttemptRepository attempts, ICurrentUserService currentUser)
    : IQueryHandler<GetSpeakingEvaluationQuery, SpeakingEvaluationResponse>
{
    public async Task<Result<SpeakingEvaluationResponse>> Handle(GetSpeakingEvaluationQuery request, CancellationToken cancellationToken)
    {
        var evaluation = await evaluations
            .GetByIdAsync(SpeakingEvaluationId.From(request.EvaluationId), cancellationToken)
            .ConfigureAwait(false);
        if (evaluation is null)
        {
            return Result.Failure<SpeakingEvaluationResponse>(SpeakingErrors.EvaluationNotFound);
        }

        var attempt = await attempts.GetByIdAsync(evaluation.SpeakingAttemptId, cancellationToken).ConfigureAwait(false);
        var isOwner = attempt is not null && attempt.UserId == request.UserId;
        var isTeacher = currentUser.Roles.Contains("Teacher");
        if (attempt is null || (!isOwner && !isTeacher))
        {
            return Result.Failure<SpeakingEvaluationResponse>(SpeakingErrors.EvaluationNotFound);
        }

        return Result.Success(ToResponse(evaluation));
    }

    internal static SpeakingEvaluationResponse ToResponse(SpeakingEvaluation evaluation) => new(
        evaluation.Id.Value, evaluation.SpeakingAttemptId.Value, evaluation.FluencyCoherence, evaluation.LexicalResource,
        evaluation.GrammaticalRangeAccuracy, evaluation.Pronunciation, evaluation.EstimatedBand, evaluation.Confidence,
        evaluation.FeedbackText, evaluation.RequiresHumanReview, evaluation.IsOverridden, evaluation.OverriddenFluencyCoherence,
        evaluation.OverriddenLexicalResource, evaluation.OverriddenGrammaticalRangeAccuracy, evaluation.OverriddenPronunciation,
        evaluation.OverriddenBand, evaluation.OverriddenBy, evaluation.OverriddenAtUtc, evaluation.OverrideReason,
        evaluation.IsReviewed, evaluation.ReviewedBy, evaluation.ReviewedAtUtc, evaluation.ReviewDecision);
}
