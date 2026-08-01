using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Queries.GetWritingEvaluation;

public sealed class GetWritingEvaluationQueryHandler(
    IWritingEvaluationRepository evaluations, IWritingSubmissionRepository submissions, ICurrentUserService currentUser)
    : IQueryHandler<GetWritingEvaluationQuery, WritingEvaluationResponse>
{
    public async Task<Result<WritingEvaluationResponse>> Handle(GetWritingEvaluationQuery request, CancellationToken cancellationToken)
    {
        var evaluation = await evaluations
            .GetByIdAsync(WritingEvaluationId.From(request.EvaluationId), cancellationToken)
            .ConfigureAwait(false);
        if (evaluation is null)
        {
            return Result.Failure<WritingEvaluationResponse>(WritingErrors.EvaluationNotFound);
        }

        var submission = await submissions.GetByIdAsync(evaluation.WritingSubmissionId, cancellationToken).ConfigureAwait(false);
        var isOwner = submission is not null && submission.UserId == request.UserId;
        var isTeacher = currentUser.Roles.Contains("Teacher");
        if (submission is null || (!isOwner && !isTeacher))
        {
            return Result.Failure<WritingEvaluationResponse>(WritingErrors.EvaluationNotFound);
        }

        return Result.Success(ToResponse(evaluation));
    }

    internal static WritingEvaluationResponse ToResponse(WritingEvaluation evaluation) => new(
        evaluation.Id.Value, evaluation.WritingSubmissionId.Value, evaluation.TaskResponse, evaluation.CoherenceCohesion,
        evaluation.LexicalResource, evaluation.GrammaticalRangeAccuracy, evaluation.EstimatedBand, evaluation.Confidence,
        evaluation.FeedbackText, evaluation.RequiresHumanReview, evaluation.IsOverridden, evaluation.OverriddenTaskResponse,
        evaluation.OverriddenCoherenceCohesion, evaluation.OverriddenLexicalResource, evaluation.OverriddenGrammaticalRangeAccuracy,
        evaluation.OverriddenBand, evaluation.OverriddenBy, evaluation.OverriddenAtUtc, evaluation.OverrideReason);
}
