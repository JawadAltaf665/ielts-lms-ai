using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.GetGrammarExercise;

public sealed class GetGrammarExerciseQueryHandler(IGrammarExerciseRepository exercises, IGrammarQuestionRepository questions)
    : IQueryHandler<GetGrammarExerciseQuery, GrammarExerciseDetailResponse>
{
    public async Task<Result<GrammarExerciseDetailResponse>> Handle(GetGrammarExerciseQuery request, CancellationToken cancellationToken)
    {
        var exerciseId = GrammarExerciseId.From(request.ExerciseId);
        var exercise = await exercises.GetVisibleByIdAsync(request.TenantId, exerciseId, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return Result.Failure<GrammarExerciseDetailResponse>(GrammarErrors.ExerciseNotFound);
        }

        var exerciseQuestions = await questions.ListByExerciseAsync(exerciseId, cancellationToken).ConfigureAwait(false);

        return Result.Success(new GrammarExerciseDetailResponse(
            exercise.Id.Value, exercise.GrammarTopicId.Value, exercise.Title, exercise.Mode.ToString(), exercise.Order,
            exerciseQuestions.Select(q => new GrammarQuestionResponse(q.Id.Value, q.Type.ToString(), q.Prompt, q.Sequence)).ToArray()));
    }
}
