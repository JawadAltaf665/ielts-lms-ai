using IELTSLms.Application.Abstractions.Authorization;
using IELTSLms.Application.Abstractions.Messaging;

namespace IELTSLms.Application.AI.Prompts.CreatePromptVersion;

/// <summary>
/// Authors a new prompt version for a feature/scope (Prompt Library &amp; Versioning, AI Architecture
/// §5). Always creates a new row scoped to the caller's tenant — it never mutates an existing version
/// (reproducibility) and never targets the global-default row (that stays seed-managed). The version
/// number is server-computed: one past whatever already exists for this exact (Feature, Scope) slot.
/// The new row is a draft (<c>IsActive = false</c>) until a separate <c>ActivatePromptVersion</c> call
/// rolls it out.
/// </summary>
public sealed record CreatePromptVersionCommand(Guid TenantId, string Feature, string Scope, string SystemTemplate, Guid? AiProviderId)
    : ICommand<AiPromptResponse>, IAuthorizeRequest
{
    public string[] RequiredRoles => ["Tenant Admin", "Content Manager"];

    public string[] RequiredPermissions => ["ai.prompt.manage"];
}
