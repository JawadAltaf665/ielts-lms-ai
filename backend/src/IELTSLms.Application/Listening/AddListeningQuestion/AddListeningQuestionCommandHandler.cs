using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Listening;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Listening.AddListeningQuestion;

public sealed class AddListeningQuestionCommandHandler(
    IListeningAudioRepository audios, IListeningQuestionRepository questions, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<AddListeningQuestionCommand, ListeningQuestionResponse>
{
    public async Task<Result<ListeningQuestionResponse>> Handle(AddListeningQuestionCommand request, CancellationToken cancellationToken)
    {
        var audioId = ListeningAudioId.From(request.AudioId);
        var audio = await audios.GetVisibleByIdAsync(request.TenantId, audioId, cancellationToken).ConfigureAwait(false);
        if (audio is null)
        {
            return Result.Failure<ListeningQuestionResponse>(ListeningErrors.AudioNotFound);
        }

        if (audio.IsGlobal)
        {
            return Result.Failure<ListeningQuestionResponse>(ListeningErrors.AudioReadOnly);
        }

        if (await questions.SequenceExistsAsync(audioId, request.Sequence, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<ListeningQuestionResponse>(ListeningErrors.DuplicateQuestionSequence);
        }

        var now = clock.UtcNow;
        var createResult = ListeningQuestion.Create(
            ListeningQuestionId.From(guids.NewGuid()), request.TenantId, audioId, request.Type, request.Prompt,
            JsonSerializer.Serialize(request.AnswerKey), request.Sequence, now);
        if (createResult.IsFailure)
        {
            return Result.Failure<ListeningQuestionResponse>(createResult.Error);
        }

        var question = createResult.Value;
        questions.Add(question);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(
            new ListeningQuestionResponse(question.Id.Value, audioId.Value, question.Type.ToString(), question.Prompt, question.Sequence));
    }
}
