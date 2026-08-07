using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Prompts;

/// <summary>Full prompt template detail for the admin management surface (ERD ENT-051).</summary>
/// <param name="Version">The prompt's business version number (<c>AiPrompt.PromptVersion</c>).</param>
/// <param name="RowVersion">Optimistic-concurrency token for <c>If-Match</c> on <c>PATCH .../activate</c>.</param>
public sealed record AiPromptResponse(
    Guid Id,
    Guid TenantId,
    Guid? AiProviderId,
    string Feature,
    int Version,
    string Scope,
    string SystemTemplate,
    bool IsActive,
    uint RowVersion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);

internal static class AiPromptResponseMapper
{
    public static AiPromptResponse ToResponse(AiPrompt prompt) => new(
        prompt.Id.Value,
        prompt.TenantId!.Value,
        prompt.AiProviderId?.Value,
        prompt.Feature,
        prompt.PromptVersion,
        prompt.Scope,
        prompt.SystemTemplate,
        prompt.IsActive,
        prompt.Version,
        prompt.CreatedAtUtc,
        prompt.UpdatedAtUtc);
}
