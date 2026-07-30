using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Vocabulary.CreateVocabularyWord;
using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Application.Vocabulary.UpdateVocabularyWord;

/// <summary>
/// Updates a tenant-owned vocabulary word. Global words (<c>TenantId == null</c>) are read-only to
/// tenants and rejected with <see cref="VocabularyErrors.WordReadOnly"/>. <c>vocabulary.manage</c> only
/// (see <see cref="CreateVocabularyWordCommand"/> remarks on enforcement status).
/// </summary>
public sealed record UpdateVocabularyWordCommand(
    Guid TenantId, Guid WordId, string Sense, string Meaning, IReadOnlyList<string> Examples,
    VocabularyFrequencyBand FrequencyBand, string? Topic, Guid? CategoryId, VocabularyDifficultyLevel DifficultyLevel)
    : ICommand<VocabularyWordResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["vocabulary.manage"];
}
