using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Common.Pagination;
using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Application.Vocabulary.GetVocabularyWords;

/// <summary>Lists vocabulary words visible to the caller's tenant (global + tenant-authored), filterable
/// by topic, difficulty level, and free-text search over word/meaning.</summary>
public sealed record GetVocabularyWordsQuery(
    Guid TenantId, string? Topic, VocabularyDifficultyLevel? DifficultyLevel, string? Search, int Page, int PageSize)
    : IQuery<PagedList<VocabularyWordResponse>>;
