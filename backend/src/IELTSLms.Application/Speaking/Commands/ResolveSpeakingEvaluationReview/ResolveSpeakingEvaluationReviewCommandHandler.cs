using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Commands.ResolveSpeakingEvaluationReview;

public sealed class ResolveSpeakingEvaluationReviewCommandHandler(
    ISpeakingEvaluationRepository evaluations, IUnitOfWork unitOfWork, IClock clock, ICurrentUserService currentUser)
    : ICommandHandler<ResolveSpeakingEvaluationReviewCommand, SpeakingEvaluationResponse>
{
    private static readonly Error NotTeacher = Error.Forbidden(
        "AUTHZ_SPEAKING_EVALUATION_RESOLVE_REVIEW_TEACHER_ONLY", "Only a teacher may resolve a speaking evaluation review.");

    public async Task<Result<SpeakingEvaluationResponse>> Handle(
        ResolveSpeakingEvaluationReviewCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Roles.Contains("Teacher"))
        {
            return Result.Failure<SpeakingEvaluationResponse>(NotTeacher);
        }

        var evaluation = await evaluations
            .GetByIdAsync(SpeakingEvaluationId.From(request.EvaluationId), cancellationToken)
            .ConfigureAwait(false);
        if (evaluation is null)
        {
            return Result.Failure<SpeakingEvaluationResponse>(SpeakingErrors.EvaluationNotFound);
        }

        var reviewResult = evaluation.MarkReviewed(request.Decision, request.ResolvedBy, clock.UtcNow);
        if (reviewResult.IsFailure)
        {
            return Result.Failure<SpeakingEvaluationResponse>(reviewResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(GetSpeakingEvaluationQueryHandler.ToResponse(evaluation));
    }
}
