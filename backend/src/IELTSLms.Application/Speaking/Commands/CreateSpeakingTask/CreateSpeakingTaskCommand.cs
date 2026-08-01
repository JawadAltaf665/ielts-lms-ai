using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Speaking.Commands.CreateSpeakingTask;

/// <summary>
/// Creates a tenant-authored speaking task (ERD ENT-030). Content-manager/teacher only per the sprint's
/// permission matrix (<c>speaking.manage</c>) — declared via <see cref="IAuthorizeRequest"/> for
/// documentation purposes; not yet enforced by <c>AuthorizationPipelineBehavior</c>, same established
/// gap as every other fine-grained permission in this codebase (mirrors <c>CreateWritingTaskCommand</c>).
/// </summary>
public sealed record CreateSpeakingTaskCommand(
    Guid TenantId, string Title, SpeakingPart Part, string PromptText, Guid? CategoryId)
    : ICommand<SpeakingTaskResponse>, IAuthorizeRequest
{
    public string[] RequiredPermissions => ["speaking.manage"];
}

public sealed record SpeakingTaskResponse(
    Guid Id, Guid? TenantId, string Title, string Part, string PromptText, Guid? CategoryId, bool IsGlobal);
