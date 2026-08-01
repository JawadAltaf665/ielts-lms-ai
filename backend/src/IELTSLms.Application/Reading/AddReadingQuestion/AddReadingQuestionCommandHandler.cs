using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Reading.AddReadingQuestion;

public sealed class AddReadingQuestionCommandHandler(
    IReadingPassageRepository passages, IReadingQuestionRepository questions, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<AddReadingQuestionCommand, ReadingQuestionResponse>
{
    public async Task<Result<ReadingQuestionResponse>> Handle(AddReadingQuestionCommand request, CancellationToken cancellationToken)
    {
        var passageId = ReadingPassageId.From(request.PassageId);
        var passage = await passages.GetVisibleByIdAsync(request.TenantId, passageId, cancellationToken).ConfigureAwait(false);
        if (passage is null)
        {
            return Result.Failure<ReadingQuestionResponse>(ReadingErrors.PassageNotFound);
        }

        if (passage.IsGlobal)
        {
            return Result.Failure<ReadingQuestionResponse>(ReadingErrors.PassageReadOnly);
        }

        if (await questions.SequenceExistsAsync(passageId, request.Sequence, excludingId: null, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<ReadingQuestionResponse>(ReadingErrors.DuplicateQuestionSequence);
        }

        var now = clock.UtcNow;
        var createResult = ReadingQuestion.Create(
            ReadingQuestionId.From(guids.NewGuid()), request.TenantId, passageId, request.Type, request.Prompt,
            JsonSerializer.Serialize(request.AnswerKey), request.Sequence, now);
        if (createResult.IsFailure)
        {
            return Result.Failure<ReadingQuestionResponse>(createResult.Error);
        }

        var question = createResult.Value;
        questions.Add(question);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new ReadingQuestionResponse(question.Id.Value, passageId.Value, question.Type.ToString(), question.Prompt, question.Sequence));
    }
}
