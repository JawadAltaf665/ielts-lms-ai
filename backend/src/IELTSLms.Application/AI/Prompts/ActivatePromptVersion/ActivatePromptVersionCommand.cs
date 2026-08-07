using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Prompts.ActivatePromptVersion;

/// <summary>
/// Rolls a specific prompt version out for its (Feature, Scope) slot — the A/B-slot assignment point.
/// Deactivates whichever other version was active in the exact same (TenantId, Feature, Scope) slot;
/// a different <see cref="Domain.AI.AiPrompt.Scope"/> for the same feature keeps its own independently
/// active version (the A/B mechanic), never touched by this call. <see cref="IfMatchVersion"/> is the
/// client's last-known <c>RowVersion</c> (API Design Standards §11 <c>If-Match</c>); a mismatch means
/// someone else changed this row since the client read it.
/// </summary>
public sealed record ActivatePromptVersionCommand(Guid TenantId, Guid PromptId, uint? IfMatchVersion)
    : ICommand<AiPromptResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Content Manager"];

    public string[] RequiredPermissions => ["ai.prompt.manage"];
}
