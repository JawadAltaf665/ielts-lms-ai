using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Grammar;

namespace IELTSLms.Application.Grammar.CreateGrammarTopic;

/// <summary>
/// Creates a tenant-authored grammar topic (ERD ENT-016). Content-manager/admin only per the sprint's
/// permission matrix (<c>grammar.manage</c>) — declared via <see cref="IAuthorizeRequest"/> for
/// documentation purposes; not yet enforced by <c>AuthorizationPipelineBehavior</c>, which only checks
/// <c>RequiredRoles</c> until fine-grained permission enforcement lands (see that type's own remarks).
/// </summary>
public sealed record CreateGrammarTopicCommand(
    Guid TenantId, string Title, string Description, GrammarTrack Track, GrammarDifficulty Difficulty, Guid? CategoryId)
    : ICommand<GrammarTopicResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["grammar.manage"];
}

public sealed record GrammarTopicResponse(
    Guid Id, Guid? TenantId, string Title, string Description, string Track, string Difficulty, Guid? CategoryId, bool IsGlobal);
