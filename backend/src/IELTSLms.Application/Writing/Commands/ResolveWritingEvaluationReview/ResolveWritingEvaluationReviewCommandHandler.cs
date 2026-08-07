using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Queries.GetWritingEvaluation;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Commands.ResolveWritingEvaluationReview;

public sealed class ResolveWritingEvaluationReviewCommandHandler(
    IWritingEvaluationRepository evaluations, IUnitOfWork unitOfWork, IClock clock, ICurrentUserService currentUser)
    : ICommandHandler<ResolveWritingEvaluationReviewCommand, WritingEvaluationResponse>
{
    private static readonly Error NotTeacher = Error.Forbidden(
        "AUTHZ_WRITING_EVALUATION_RESOLVE_REVIEW_TEACHER_ONLY", "Only a teacher may resolve a writing evaluation review.");

    public async Task<Result<WritingEvaluationResponse>> Handle(
        ResolveWritingEvaluationReviewCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Roles.Contains("Teacher"))
        {
            return Result.Failure<WritingEvaluationResponse>(NotTeacher);
        }

        var evaluation = await evaluations
            .GetByIdAsync(WritingEvaluationId.From(request.EvaluationId), cancellationToken)
            .ConfigureAwait(false);
        if (evaluation is null)
        {
            return Result.Failure<WritingEvaluationResponse>(WritingErrors.EvaluationNotFound);
        }

        var reviewResult = evaluation.MarkReviewed(request.Decision, request.ResolvedBy, clock.UtcNow);
        if (reviewResult.IsFailure)
        {
            return Result.Failure<WritingEvaluationResponse>(reviewResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(GetWritingEvaluationQueryHandler.ToResponse(evaluation));
    }
}
