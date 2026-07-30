using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Application.Vocabulary.GradeVocabularyReview;

namespace IELTSLms.Application.Vocabulary.GetNextReviewBatch;

/// <summary>Gets the caller's next due review batch: words whose <c>VocabularyProgress.NextReviewAtUtc</c>
/// has passed, filled out with new (never-reviewed) words if fewer than a full batch is due.</summary>
public sealed record GetNextReviewBatchQuery(Guid TenantId, Guid UserId, int? BatchSize)
    : IQuery<IReadOnlyList<VocabularyReviewItemResponse>>;

public sealed record VocabularyReviewItemResponse(VocabularyWordResponse Word, VocabularyProgressResponse? Progress);
