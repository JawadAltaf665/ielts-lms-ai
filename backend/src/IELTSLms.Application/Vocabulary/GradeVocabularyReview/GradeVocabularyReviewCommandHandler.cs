using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Vocabulary;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Vocabulary.GradeVocabularyReview;

/// <remarks>
/// Concurrent grading of the same (user, word) row is guarded by the entity's natural EF/xmin
/// optimistic-concurrency check — same as <c>UpdateGrammarTopicCommandHandler</c> — which is what
/// satisfies the sprint's "concurrent grade submissions don't corrupt the schedule (Version conflict →
/// 409)" requirement without needing application-level idempotency-key infrastructure that doesn't
/// exist anywhere yet.
/// </remarks>
public sealed class GradeVocabularyReviewCommandHandler(
    IVocabularyWordRepository words, IVocabularyProgressRepository progressRepository, IUnitOfWork unitOfWork, IClock clock,
    IGuidProvider guids) : ICommandHandler<GradeVocabularyReviewCommand, VocabularyProgressResponse>
{
    public async Task<Result<VocabularyProgressResponse>> Handle(GradeVocabularyReviewCommand request, CancellationToken cancellationToken)
    {
        var wordId = VocabularyWordId.From(request.WordId);
        var word = await words.GetVisibleByIdAsync(request.TenantId, wordId, cancellationToken).ConfigureAwait(false);
        if (word is null)
        {
            return Result.Failure<VocabularyProgressResponse>(VocabularyErrors.WordNotFound);
        }

        var now = clock.UtcNow;

        // A word new to the learner (no progress row) is treated as due immediately and seeded with
        // EaseFactor 2.5 / RepetitionCount 0 on first grade (Sprint-2-Vocabulary-Module.md business rules).
        var progress = await progressRepository.GetByUserAndWordAsync(request.TenantId, request.UserId, wordId, cancellationToken)
            .ConfigureAwait(false);
        if (progress is null)
        {
            progress = VocabularyProgress.Create(VocabularyProgressId.From(guids.NewGuid()), request.TenantId, request.UserId, wordId, now);
            progressRepository.Add(progress);
        }

        progress.Grade(request.Grade, now);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(ToResponse(progress));
    }

    internal static VocabularyProgressResponse ToResponse(VocabularyProgress progress) => new(
        progress.VocabularyWordId.Value, progress.EaseFactor, progress.IntervalDays, progress.RepetitionCount,
        progress.NextReviewAtUtc, progress.LastGrade);
}
