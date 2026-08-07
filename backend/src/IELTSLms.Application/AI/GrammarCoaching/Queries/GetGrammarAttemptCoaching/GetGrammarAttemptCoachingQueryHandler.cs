using System.Text.Json;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.Grammar;
using IELTSLms.Application.Grammar.SubmitGrammarAttempt;
using IELTSLms.Domain.Grammar;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.AI.GrammarCoaching.Queries.GetGrammarAttemptCoaching;

public sealed class GetGrammarAttemptCoachingQueryHandler(
    IGrammarAttemptRepository attempts, IGrammarQuestionRepository questions, ISender sender)
    : IQueryHandler<GetGrammarAttemptCoachingQuery, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(
        GetGrammarAttemptCoachingQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(GrammarAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<AiOperationAcceptedResponse>(GrammarErrors.AttemptNotFound);
        }

        var exerciseQuestions = await questions
            .ListByExerciseAsync(attempt.GrammarExerciseId, cancellationToken).ConfigureAwait(false);
        var questionsById = exerciseQuestions.ToDictionary(q => q.Id.Value);

        var answers = JsonSerializer.Deserialize<GrammarAttemptAnswerResponse[]>(attempt.AnswersJson) ?? [];
        var wrongAnswers = answers.Where(a => !a.IsCorrect).ToArray();
        if (wrongAnswers.Length == 0)
        {
            return Result.Failure<AiOperationAcceptedResponse>(GrammarErrors.NoIncorrectAnswers);
        }

        var text = ComposeWrongAnswerText(wrongAnswers, questionsById);
        return await sender
            .Send(new RequestGrammarCoachingCommand(request.TenantId, request.UserId, text), cancellationToken)
            .ConfigureAwait(false);
    }

    private static string ComposeWrongAnswerText(
        GrammarAttemptAnswerResponse[] wrongAnswers, Dictionary<Guid, GrammarQuestion> questionsById)
    {
        var blocks = new List<string>(wrongAnswers.Length);
        foreach (var answer in wrongAnswers)
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question))
            {
                continue;
            }

            var correctAlternatives = string.Join(" / ", ParseAnswerKey(question.AnswerKey));
            blocks.Add(
                $"Question: {question.Prompt}\n" +
                $"Submitted answer: {answer.SubmittedAnswer ?? "(none)"}\n" +
                $"Correct answer: {correctAlternatives}");
        }

        return string.Join("\n\n", blocks);
    }

    private static string[] ParseAnswerKey(string answerKeyJson)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(answerKeyJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
