using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;
using IELTSLms.Domain.Speaking;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Speaking.Commands.OverrideSpeakingEvaluation;

public sealed class OverrideSpeakingEvaluationCommandHandler(
    ISpeakingEvaluationRepository evaluations, IUnitOfWork unitOfWork, IClock clock, ICurrentUserService currentUser)
    : ICommandHandler<OverrideSpeakingEvaluationCommand, SpeakingEvaluationResponse>
{
    private static readonly Error NotTeacher = Error.Forbidden(
        "AUTHZ_SPEAKING_EVALUATION_OVERRIDE_TEACHER_ONLY", "Only a teacher may override a speaking evaluation.");

    public async Task<Result<SpeakingEvaluationResponse>> Handle(
        OverrideSpeakingEvaluationCommand request, CancellationToken cancellationToken)
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

        var overrideResult = evaluation.Override(
            request.FluencyCoherence, request.LexicalResource, request.GrammaticalRangeAccuracy, request.Pronunciation,
            request.Reason, request.OverriddenBy, clock.UtcNow);
        if (overrideResult.IsFailure)
        {
            return Result.Failure<SpeakingEvaluationResponse>(overrideResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(GetSpeakingEvaluationQueryHandler.ToResponse(evaluation));
    }
}
