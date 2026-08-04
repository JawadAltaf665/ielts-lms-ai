using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.SentenceBuilder.Queries.GetSentenceExercise;

public sealed class GetSentenceExerciseQueryHandler(ISentenceExerciseRepository exercises)
    : IQueryHandler<GetSentenceExerciseQuery, SentenceExerciseResponse>
{
    public async Task<Result<SentenceExerciseResponse>> Handle(GetSentenceExerciseQuery request, CancellationToken cancellationToken)
    {
        var exercise = await exercises
            .GetVisibleByIdAsync(request.TenantId, SentenceExerciseId.From(request.ExerciseId), cancellationToken)
            .ConfigureAwait(false);
        if (exercise is null)
        {
            return Result.Failure<SentenceExerciseResponse>(SentenceBuilderErrors.ExerciseNotFound);
        }

        return Result.Success(CreateSentenceExerciseCommandHandler.ToResponse(exercise));
    }
}
