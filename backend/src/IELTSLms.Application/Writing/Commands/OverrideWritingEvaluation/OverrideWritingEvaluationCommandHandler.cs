using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Writing.Queries.GetWritingEvaluation;
using IELTSLms.Domain.Writing;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Writing.Commands.OverrideWritingEvaluation;

public sealed class OverrideWritingEvaluationCommandHandler(
    IWritingEvaluationRepository evaluations, IUnitOfWork unitOfWork, IClock clock, ICurrentUserService currentUser)
    : ICommandHandler<OverrideWritingEvaluationCommand, WritingEvaluationResponse>
{
    private static readonly Error NotTeacher = Error.Forbidden(
        "AUTHZ_WRITING_EVALUATION_OVERRIDE_TEACHER_ONLY", "Only a teacher may override a writing evaluation.");

    public async Task<Result<WritingEvaluationResponse>> Handle(
        OverrideWritingEvaluationCommand request, CancellationToken cancellationToken)
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

        var overrideResult = evaluation.Override(
            request.TaskResponse, request.CoherenceCohesion, request.LexicalResource, request.GrammaticalRangeAccuracy,
            request.Reason, request.OverriddenBy, clock.UtcNow);
        if (overrideResult.IsFailure)
        {
            return Result.Failure<WritingEvaluationResponse>(overrideResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(GetWritingEvaluationQueryHandler.ToResponse(evaluation));
    }
}
