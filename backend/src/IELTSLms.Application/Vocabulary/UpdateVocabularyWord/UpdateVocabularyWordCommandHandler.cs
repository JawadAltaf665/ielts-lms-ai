using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.UpdateVocabularyWord;

/// <remarks>
/// Concurrency here relies on the natural EF/xmin optimistic-concurrency check (load, mutate, save
/// within one request) — same as <c>UpdateGrammarTopicCommandHandler</c>. A client-supplied
/// <c>If-Match</c>/<c>Version</c> check would need generic ETag middleware that doesn't exist anywhere
/// yet; ponytail: add if a real staleness bug shows up.
/// </remarks>
public sealed class UpdateVocabularyWordCommandHandler(IVocabularyWordRepository words, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<UpdateVocabularyWordCommand, VocabularyWordResponse>
{
    public async Task<Result<VocabularyWordResponse>> Handle(UpdateVocabularyWordCommand request, CancellationToken cancellationToken)
    {
        var word = await words.GetVisibleByIdAsync(request.TenantId, VocabularyWordId.From(request.WordId), cancellationToken)
            .ConfigureAwait(false);
        if (word is null)
        {
            return Result.Failure<VocabularyWordResponse>(VocabularyErrors.WordNotFound);
        }

        if (word.IsGlobal)
        {
            return Result.Failure<VocabularyWordResponse>(VocabularyErrors.WordReadOnly);
        }

        if (!string.Equals(word.Sense, request.Sense, StringComparison.Ordinal)
            && await words.WordSenseExistsAsync(word.TenantId, word.Word, request.Sense, word.Id, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<VocabularyWordResponse>(VocabularyErrors.WordTaken);
        }

        var updateResult = word.Update(
            request.Sense, request.Meaning, JsonSerializer.Serialize(request.Examples), request.FrequencyBand, request.Topic,
            request.CategoryId, request.DifficultyLevel, clock.UtcNow);
        if (updateResult.IsFailure)
        {
            return Result.Failure<VocabularyWordResponse>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(CreateVocabularyWordCommandHandler.ToResponse(word));
    }
}
