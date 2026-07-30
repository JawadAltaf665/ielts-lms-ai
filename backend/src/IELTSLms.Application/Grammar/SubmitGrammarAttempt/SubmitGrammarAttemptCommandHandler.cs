using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Grammar.SubmitGrammarAttempt;

public sealed class SubmitGrammarAttemptCommandHandler(
    IGrammarExerciseRepository exercises, IGrammarQuestionRepository questions, IGrammarAttemptRepository attempts,
    IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<SubmitGrammarAttemptCommand, GrammarAttemptResponse>
{
    public async Task<Result<GrammarAttemptResponse>> Handle(SubmitGrammarAttemptCommand request, CancellationToken cancellationToken)
    {
        var exerciseId = GrammarExerciseId.From(request.ExerciseId);
        var exercise = await exercises.GetVisibleByIdAsync(request.TenantId, exerciseId, cancellationToken).ConfigureAwait(false);
        if (exercise is null)
        {
            return Result.Failure<GrammarAttemptResponse>(GrammarErrors.ExerciseNotFound);
        }

        var exerciseQuestions = await questions.ListByExerciseAsync(exerciseId, cancellationToken).ConfigureAwait(false);
        if (exerciseQuestions.Count == 0)
        {
            return Result.Failure<GrammarAttemptResponse>(GrammarErrors.NoQuestionsToGrade);
        }

        var submittedByQuestion = request.Answers.ToDictionary(a => a.QuestionId, a => a.Answer);

        var graded = exerciseQuestions
            .Select(question =>
            {
                var submitted = submittedByQuestion.GetValueOrDefault(question.Id.Value);
                return new GrammarAttemptAnswerResponse(question.Id.Value, submitted, question.Grade(submitted));
            })
            .ToArray();

        var rawScore = graded.Count(a => a.IsCorrect);
        var maxScore = exerciseQuestions.Count;
        var now = clock.UtcNow;

        var attempt = GrammarAttempt.Submit(
            GrammarAttemptId.From(guids.NewGuid()), request.TenantId, request.UserId, exerciseId, exercise.GrammarTopicId.Value,
            JsonSerializer.Serialize(graded), rawScore, maxScore, now);

        attempts.Add(attempt);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new GrammarAttemptResponse(attempt.Id.Value, exerciseId.Value, rawScore, maxScore, graded, now));
    }
}
