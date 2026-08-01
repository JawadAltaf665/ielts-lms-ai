using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.Grammar.DeleteGrammarTopic;

/// <summary>Soft-deletes a tenant-owned grammar topic. Global topics are read-only to tenants. <c>grammar.manage</c> only.</summary>
public sealed record DeleteGrammarTopicCommand(Guid TenantId, Guid TopicId) : ICommand, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["grammar.manage"];
}
