using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Vocabulary.GradeVocabularyReview;

/// <summary>
/// Grades one review of a word (0-5 SM-2 quality) and advances its spaced-repetition schedule. Any
/// authenticated learner may grade their own reviews — no <c>vocabulary.manage</c> permission needed
/// (mirrors <c>SubmitGrammarAttemptCommand</c>, which is likewise self-service). Raises
/// <c>VocabularyCompleted</c> on success.
/// </summary>
public sealed record GradeVocabularyReviewCommand(Guid TenantId, Guid UserId, Guid WordId, int Grade)
    : ICommand<VocabularyProgressResponse>;

public sealed record VocabularyProgressResponse(
    Guid WordId, decimal EaseFactor, int IntervalDays, int RepetitionCount, DateTimeOffset? NextReviewAtUtc, int? LastGrade);
