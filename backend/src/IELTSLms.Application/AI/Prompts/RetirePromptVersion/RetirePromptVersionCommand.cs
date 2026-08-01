using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Prompts.RetirePromptVersion;

/// <summary>
/// Soft-deletes a prompt version. Rejected while the version is currently active for its feature/scope
/// (fail closed — a feature must never be left with no active prompt); an already-retired version
/// referenced by historical <see cref="Domain.AI.AiResponse"/> rows stays soft-deleted, never hard-deleted,
/// so it remains readable for reproducibility.
/// </summary>
public sealed record RetirePromptVersionCommand(Guid TenantId, Guid PromptId) : ICommand, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Content Manager"];

    public string[] RequiredPermissions => ["ai.prompt.manage"];
}
