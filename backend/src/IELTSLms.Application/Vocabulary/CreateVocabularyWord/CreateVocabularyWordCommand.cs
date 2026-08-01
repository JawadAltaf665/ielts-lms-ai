using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Vocabulary;

namespace IELTSLms.Application.Vocabulary.CreateVocabularyWord;

/// <summary>
/// Creates a tenant-authored vocabulary word (ERD ENT-017). Content-manager/admin only per the sprint's
/// permission matrix (<c>vocabulary.manage</c>) — declared via <see cref="IAuthorizeRequest"/> for
/// documentation purposes; not yet enforced by <c>AuthorizationPipelineBehavior</c>, which only checks
/// <c>RequiredRoles</c> until fine-grained permission enforcement lands (mirrors
/// <c>CreateGrammarTopicCommand</c>'s remarks on enforcement status).
/// </summary>
public sealed record CreateVocabularyWordCommand(
    Guid TenantId, string Word, string Sense, string Meaning, IReadOnlyList<string> Examples,
    VocabularyFrequencyBand FrequencyBand, string? Topic, Guid? CategoryId, VocabularyDifficultyLevel DifficultyLevel)
    : ICommand<VocabularyWordResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["vocabulary.manage"];
}

public sealed record VocabularyWordResponse(
    Guid Id, Guid? TenantId, string Word, string Sense, string Meaning, IReadOnlyList<string> Examples, string FrequencyBand,
    string? Topic, Guid? CategoryId, string DifficultyLevel, bool IsGlobal);
