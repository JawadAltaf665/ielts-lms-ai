using System.Text.Json;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.AI.RequestListeningCoaching;
using IELTSLms.Application.Listening;
using IELTSLms.Application.Listening.SubmitListeningAttempt;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.AI.ListeningCoaching.Queries.GetListeningAttemptCoaching;

public sealed class GetListeningAttemptCoachingQueryHandler(
    IListeningAttemptRepository attempts, IListeningAudioRepository audios, IListeningQuestionRepository questions, ISender sender)
    : IQueryHandler<GetListeningAttemptCoachingQuery, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(
        GetListeningAttemptCoachingQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attempts.GetByIdAsync(ListeningAttemptId.From(request.AttemptId), cancellationToken).ConfigureAwait(false);
        if (attempt is null || attempt.UserId != request.UserId)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ListeningErrors.AttemptNotFound);
        }

        if (attempt.AnswersJson is null)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ListeningErrors.AttemptNotSubmitted);
        }

        var audio = await audios
            .GetVisibleByIdAsync(request.TenantId, attempt.ListeningAudioId, cancellationToken).ConfigureAwait(false);
        if (audio is null)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ListeningErrors.AudioNotFound);
        }

        var audioQuestions = await questions
            .ListByAudioAsync(attempt.ListeningAudioId, cancellationToken).ConfigureAwait(false);
        var questionsById = audioQuestions.ToDictionary(q => q.Id.Value);

        var answers = JsonSerializer.Deserialize<ListeningAttemptAnswerResponse[]>(attempt.AnswersJson) ?? [];
        var wrongAnswers = answers.Where(a => !a.IsCorrect).ToArray();
        if (wrongAnswers.Length == 0)
        {
            return Result.Failure<AiOperationAcceptedResponse>(ListeningErrors.NoIncorrectAnswers);
        }

        var text = ComposeGroundedText(audio, wrongAnswers, questionsById);
        return await sender
            .Send(new RequestListeningCoachingCommand(request.TenantId, request.UserId, text), cancellationToken)
            .ConfigureAwait(false);
    }

    private static string ComposeGroundedText(
        ListeningAudio audio, ListeningAttemptAnswerResponse[] wrongAnswers, Dictionary<Guid, ListeningQuestion> questionsById)
    {
        var transcript = audio.TranscriptText is { Length: > 0 } ? audio.TranscriptText : "(no transcript available)";
        var blocks = new List<string>(wrongAnswers.Length + 1) { $"Audio: {audio.Title}\nTranscript: {transcript}" };
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
