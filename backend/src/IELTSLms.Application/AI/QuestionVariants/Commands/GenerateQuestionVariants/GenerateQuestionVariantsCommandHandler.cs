using System.Text.Json;
using IELTSLms.Abstractions.Jobs;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.AI.RequestGrammarCoaching;
using IELTSLms.Application.Listening;
using IELTSLms.Application.Reading;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI.QuestionVariants.Commands.GenerateQuestionVariants;

public sealed class GenerateQuestionVariantsCommandHandler(
    IReadingPassageRepository passages, IReadingQuestionRepository readingQuestions,
    IListeningAudioRepository audios, IListeningQuestionRepository listeningQuestions,
    IAiOperationRepository operations, IUnitOfWork unitOfWork, IBackgroundJobService backgroundJobs,
    IClock clock, IGuidProvider guids)
    : ICommandHandler<GenerateQuestionVariantsCommand, AiOperationAcceptedResponse>
{
    public async Task<Result<AiOperationAcceptedResponse>> Handle(
        GenerateQuestionVariantsCommand request, CancellationToken cancellationToken)
    {
        var composed = request.SourceType == QuestionVariantSourceType.Reading
            ? await ComposeForReadingAsync(request, cancellationToken).ConfigureAwait(false)
            : await ComposeForListeningAsync(request, cancellationToken).ConfigureAwait(false);
        if (composed.IsFailure)
        {
            return Result.Failure<AiOperationAcceptedResponse>(composed.Error);
        }

        var kind = request.SourceType == QuestionVariantSourceType.Reading
            ? AiFeatures.ReadingQuestionVariants
            : AiFeatures.ListeningQuestionVariants;

        var now = clock.UtcNow;
        var operation = AiOperation.Enqueue(
            AiOperationId.From(guids.NewGuid()), request.TenantId, request.UserId, kind, composed.Value, now);

        operations.Add(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var operationId = operation.Id.Value;
        var tenantId = request.TenantId;
        var sourceType = request.SourceType;
        var sourceQuestionId = request.SourceQuestionId;
        var difficultyDelta = request.DifficultyDelta;
        backgroundJobs.Enqueue<GenerateQuestionVariantsJob>(
            job => job.ExecuteAsync(tenantId, operationId, sourceType, sourceQuestionId, difficultyDelta, CancellationToken.None));

        return Result.Success(new AiOperationAcceptedResponse(operationId, "queued"));
    }

    private async Task<Result<string>> ComposeForReadingAsync(GenerateQuestionVariantsCommand request, CancellationToken cancellationToken)
    {
        var passage = await passages
            .GetVisibleByIdAsync(request.TenantId, ReadingPassageId.From(request.ContainerId), cancellationToken).ConfigureAwait(false);
        if (passage is null)
        {
            return Result.Failure<string>(ReadingErrors.PassageNotFound);
        }

        if (passage.IsGlobal)
        {
            return Result.Failure<string>(ReadingErrors.PassageReadOnly);
        }

        var question = (await readingQuestions
                .ListByPassageAsync(passage.Id, cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(q => q.Id.Value == request.SourceQuestionId);
        if (question is null)
        {
            return Result.Failure<string>(QuestionVariantErrors.SourceQuestionNotFound);
        }

        return Result.Success(ComposeText(
            "Passage", passage.Title, passage.PassageText, question.Type.ToString(), question.Prompt,
            ParseAnswerKey(question.AnswerKey), request.DifficultyDelta));
    }

    private async Task<Result<string>> ComposeForListeningAsync(GenerateQuestionVariantsCommand request, CancellationToken cancellationToken)
    {
        var audio = await audios
            .GetVisibleByIdAsync(request.TenantId, ListeningAudioId.From(request.ContainerId), cancellationToken).ConfigureAwait(false);
        if (audio is null)
        {
            return Result.Failure<string>(ListeningErrors.AudioNotFound);
        }

        if (audio.IsGlobal)
        {
            return Result.Failure<string>(ListeningErrors.AudioReadOnly);
        }

        var question = (await listeningQuestions
                .ListByAudioAsync(audio.Id, cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(q => q.Id.Value == request.SourceQuestionId);
        if (question is null)
        {
            return Result.Failure<string>(QuestionVariantErrors.SourceQuestionNotFound);
        }

        var transcript = audio.TranscriptText is { Length: > 0 } ? audio.TranscriptText : "(no transcript available)";
        return Result.Success(ComposeText(
            "Audio transcript", audio.Title, transcript, question.Type.ToString(), question.Prompt,
            ParseAnswerKey(question.AnswerKey), request.DifficultyDelta));
    }

    private static string ComposeText(
        string groundingLabel, string title, string groundingText, string questionType, string prompt,
        string[] answerKey, int difficultyDelta) =>
        $"{groundingLabel}: {title}\n{groundingText}\n\n" +
        $"Source question type: {questionType}\n" +
        $"Source question prompt: {prompt}\n" +
        $"Source answer key: {string.Join(" / ", answerKey)}\n" +
        $"Requested difficulty adjustment: {(difficultyDelta >= 0 ? "+" : string.Empty)}{difficultyDelta}";

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
