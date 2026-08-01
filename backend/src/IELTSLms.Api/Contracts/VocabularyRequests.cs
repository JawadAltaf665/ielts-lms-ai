namespace IELTSLms.Api.Contracts;

public sealed record CreateVocabularyWordRequest(
    string Word, string Sense, string Meaning, IReadOnlyList<string> Examples, string FrequencyBand, string? Topic,
    Guid? CategoryId, string DifficultyLevel);

public sealed record UpdateVocabularyWordRequest(
    string Sense, string Meaning, IReadOnlyList<string> Examples, string FrequencyBand, string? Topic, Guid? CategoryId,
    string DifficultyLevel);

public sealed record GetVocabularyWordsQueryParams(string? Topic, string? DifficultyLevel, string? Q, int? Page, int? PageSize);

public sealed record GetNextReviewBatchQueryParams(int? BatchSize);

public sealed record GradeVocabularyReviewRequest(int Grade);
