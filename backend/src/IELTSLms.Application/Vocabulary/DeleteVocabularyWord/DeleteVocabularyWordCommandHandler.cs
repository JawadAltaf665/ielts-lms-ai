using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.DeleteVocabularyWord;

public sealed class DeleteVocabularyWordCommandHandler(IVocabularyWordRepository words, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<DeleteVocabularyWordCommand>
{
    public async Task<Result> Handle(DeleteVocabularyWordCommand request, CancellationToken cancellationToken)
    {
        var word = await words.GetVisibleByIdAsync(request.TenantId, VocabularyWordId.From(request.WordId), cancellationToken)
            .ConfigureAwait(false);
        if (word is null)
        {
            return Result.Failure(VocabularyErrors.WordNotFound);
        }

        if (word.IsGlobal)
        {
            return Result.Failure(VocabularyErrors.WordReadOnly);
        }

        word.SoftDelete(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
