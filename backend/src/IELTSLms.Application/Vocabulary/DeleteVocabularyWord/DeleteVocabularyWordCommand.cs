using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Vocabulary.DeleteVocabularyWord;

/// <summary>Soft-deletes a tenant-owned vocabulary word. <c>vocabulary.manage</c> only.</summary>
public sealed record DeleteVocabularyWordCommand(Guid TenantId, Guid WordId) : ICommand, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["vocabulary.manage"];
}
