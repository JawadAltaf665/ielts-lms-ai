using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;

/// <summary>
/// Creates a tenant-authored sentence-construction exercise (ERD ENT-019). Content-manager/admin only
/// per the sprint's permission matrix (<c>sentence-builder.manage</c>) — declared via
/// <see cref="IAuthorizeRequest"/> for documentation purposes; not yet enforced by
/// <c>AuthorizationPipelineBehavior</c>, which only checks <c>RequiredRoles</c> (mirrors Grammar's
/// <c>CreateGrammarTopicCommand</c>).
/// </summary>
public sealed record CreateSentenceExerciseCommand(Guid TenantId, string Prompt, IReadOnlyList<string> TargetPattern, SentenceDifficulty Difficulty)
    : ICommand<SentenceExerciseResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["sentence-builder.manage"];
}

public sealed record SentenceExerciseResponse(
    Guid Id, Guid? TenantId, string Prompt, IReadOnlyList<string> TargetPattern, string Difficulty, bool IsGlobal);
