using System.Text.Json;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestReadingCoaching;
using IELTSLms.Application.Reading;
using IELTSLms.Application.Reading.SubmitReadingAttempt;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.AI.ReadingCoaching.Queries.GetReadingAttemptCoaching;

public sealed class GetReadingAttemptCoachingQueryHandler(
    IReadingAttemptRepository attempts, IReadingPassageRepository passages, IReadingQuestionRepository questions, ISender sender)
    : IQueryHandler<GetReadingAttemptCoachingQuery, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(
        GetReadingAttemptCoachingQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ReadingAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ReadingErrors.AttemptNotFound);
        }

        if (attempt.AnswersJson is null)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ReadingErrors.AttemptNotSubmitted);
        }

        var passage = await passages
            .GetVisibleByIdAsync(request.TenantId, attempt.ReadingPassageId, cancellationToken).ConfigureAwait(false);
        if (passage is null)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ReadingErrors.PassageNotFound);
        }

        var passageQuestions = await questions
            .ListByPassageAsync(attempt.ReadingPassageId, cancellationToken).ConfigureAwait(false);
        var questionsById = passageQuestions.ToDictionary(q => q.Id.Value);

        var answers = JsonSerializer.Deserialize<ReadingAttemptAnswerResponse[]>(attempt.AnswersJson) ?? [];
        var wrongAnswers = answers.Where(a => !a.IsCorrect).ToArray();
        if (wrongAnswers.Length == 0)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ReadingErrors.NoIncorrectAnswers);
        }

        var text = ComposeGroundedText(passage, wrongAnswers, questionsById);
        return await sender
            .Send(new RequestReadingCoachingCommand(request.TenantId, request.UserId, text), cancellationToken)
            .ConfigureAwait(false);
    }

    private static string ComposeGroundedText(
        ReadingPassage passage, ReadingAttemptAnswerResponse[] wrongAnswers, Dictionary<Guid, ReadingQuestion> questionsById)
    {
        var blocks = new List<string>(wrongAnswers.Length + 1) { $"Passage: {passage.Title}\n{passage.PassageText}" };
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
