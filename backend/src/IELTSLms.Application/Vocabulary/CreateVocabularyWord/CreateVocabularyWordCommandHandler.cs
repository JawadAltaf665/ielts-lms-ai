using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.CreateVocabularyWord;

public sealed class CreateVocabularyWordCommandHandler(
    IVocabularyWordRepository words, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids)
    : ICommandHandler<CreateVocabularyWordCommand, VocabularyWordResponse>
{
    public async Task<Result<VocabularyWordResponse>> Handle(CreateVocabularyWordCommand request, CancellationToken cancellationToken)
    {
        // Words created through the tenant-facing API are always tenant-owned; global-catalog words
        // (TenantId null) are seeded content, not authored here.
        if (await words.WordSenseExistsAsync(request.TenantId, request.Word, request.Sense, excludingId: null, cancellationToken)
            .ConfigureAwait(false))
        {
            return Result.Failure<VocabularyWordResponse>(VocabularyErrors.WordTaken);
        }

        var now = clock.UtcNow;
        var createResult = VocabularyWord.Create(
            VocabularyWordId.From(guids.NewGuid()), request.TenantId, request.Word, request.Sense, request.Meaning,
            JsonSerializer.Serialize(request.Examples), request.FrequencyBand, request.Topic, request.CategoryId,
            request.DifficultyLevel, now);

        if (createResult.IsFailure)
        {
            return Result.Failure<VocabularyWordResponse>(createResult.Error);
        }

        var word = createResult.Value;
        words.Add(word);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(word));
    }

    internal static VocabularyWordResponse ToResponse(VocabularyWord word) => new(
        word.Id.Value, word.TenantId, word.Word, word.Sense, word.Meaning,
        JsonSerializer.Deserialize<string[]>(word.ExamplesJson) ?? [], word.FrequencyBand.ToString(), word.Topic,
        word.CategoryId, word.DifficultyLevel.ToString(), word.IsGlobal);
}
