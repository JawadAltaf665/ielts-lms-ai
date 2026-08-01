using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Grammar.CreateGrammarTopic;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Grammar.UpdateGrammarTopic;

/// <summary>
/// Updates a tenant-owned grammar topic. Global topics (<c>TenantId == null</c>) are read-only to
/// tenants and rejected with <see cref="GrammarErrors.TopicReadOnly"/>. <c>grammar.manage</c> only
/// (see <see cref="CreateGrammarTopic.CreateGrammarTopicCommand"/> remarks on enforcement status).
/// </summary>
public sealed record UpdateGrammarTopicCommand(
    Guid TenantId, Guid TopicId, string Title, string Description, GrammarTrack Track, GrammarDifficulty Difficulty,
    Guid? CategoryId) : ICommand<GrammarTopicResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["grammar.manage"];
}
