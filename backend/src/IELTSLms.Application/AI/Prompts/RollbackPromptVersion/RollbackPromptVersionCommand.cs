using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Prompts.RollbackPromptVersion;

/// <summary>
/// Rolls a feature/scope back to the version that was active immediately before the current one
/// (Agentic AI Architecture §9.2 Prompt Lifecycle). A governed, audited action — <see cref="Reason"/>
/// is required and recorded via the existing <c>AuditLog</c> mechanism (not a new logging path), per
/// the sprint's guardrail against inventing a second audit trail.
/// </summary>
public sealed record RollbackPromptVersionCommand(Guid TenantId, Guid PromptId, Guid ActorUserId, string Reason)
    : ICommand<AiPromptResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Content Manager"];

    public string[] RequiredPermissions => ["ai.prompt.manage"];
}
