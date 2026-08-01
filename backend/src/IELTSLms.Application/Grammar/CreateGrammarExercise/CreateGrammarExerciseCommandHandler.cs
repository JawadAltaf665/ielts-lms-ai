using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.CreateGrammarExercise;

public sealed class CreateGrammarExerciseCommandHandler(
    IGrammarTopicRepository topics, IGrammarExerciseRepository exercises, IGrammarQuestionRepository questions,
    IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateGrammarExerciseCommand, GrammarExerciseResponse>
{
    public async Task<Result<GrammarExerciseResponse>> Handle(CreateGrammarExerciseCommand request, CancellationToken cancellationToken)
    {
        var topicId = GrammarTopicId.From(request.TopicId);
        var topic = await topics.GetVisibleByIdAsync(request.TenantId, topicId, cancellationToken).ConfigureAwait(false);
        if (topic is null)
        {
            return Result.Failure<GrammarExerciseResponse>(GrammarErrors.TopicNotFound);
        }

        if (HasDuplicateSequences(request.Questions))
        {
            return Result.Failure<GrammarExerciseResponse>(GrammarErrors.DuplicateQuestionSequence);
        }

        var now = clock.UtcNow;

        // Exercises authored through this API are always tenant-owned, even under a global topic
        // (the topic itself stays read-only; adding tenant content under it is not a topic mutation).
        var exerciseResult = GrammarExercise.Create(
            GrammarExerciseId.From(guids.NewGuid()), request.TenantId, topicId, request.Title, request.Mode, request.Order, now);
        if (exerciseResult.IsFailure)
        {
            return Result.Failure<GrammarExerciseResponse>(exerciseResult.Error);
        }

        var exercise = exerciseResult.Value;
        exercises.Add(exercise);

        foreach (var questionRequest in request.Questions)
        {
            var questionResult = GrammarQuestion.Create(
                GrammarQuestionId.From(guids.NewGuid()), request.TenantId, exercise.Id, questionRequest.Type,
                questionRequest.Prompt, JsonSerializer.Serialize(questionRequest.AnswerKey), questionRequest.Sequence, now);
            if (questionResult.IsFailure)
            {
                return Result.Failure<GrammarExerciseResponse>(questionResult.Error);
            }

            questions.Add(questionResult.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new GrammarExerciseResponse(
            exercise.Id.Value, exercise.GrammarTopicId.Value, exercise.Title, exercise.Mode.ToString(), exercise.Order,
            request.Questions.Count));
    }

    private static bool HasDuplicateSequences(IReadOnlyList<CreateGrammarQuestionRequest> questions) =>
        questions.Select(q => q.Sequence).Distinct().Count() != questions.Count;
}
