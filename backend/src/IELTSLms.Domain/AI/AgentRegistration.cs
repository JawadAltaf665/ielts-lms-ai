using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.AI;

/// <summary>
/// Persisted catalog entry for a platform AI agent (Sprint 10, Agentic AI Architecture §4).
/// Global reference data — not tenant-scoped (agents are a platform capability, not a per-tenant
/// concept). The registry is a discovery/governance catalog only; it does not grant access — invocation
/// still goes through the existing <c>Features</c>/permission checks (business rule from sprint file).
///
/// Agents are registered once per deployment and updated via <see cref="Deprecate"/> / <see cref="Enable"/>
/// status transitions; the seeder upserts by <see cref="AgentKey"/> so this is idempotent on restarts.
/// </summary>
public sealed class AgentRegistration : AuditableEntity<AgentRegistrationId>
{
    /// <summary>Maximum length of the slug identifier (e.g. <c>"writing-examiner"</c>).</summary>
    public const int AgentKeyMaxLength = 64;

    /// <summary>Maximum length of the human-readable display name.</summary>
    public const int DisplayNameMaxLength = 128;

    private AgentRegistration(
        AgentRegistrationId id,
        string agentKey,
        string displayName,
        string capabilitiesJson,
        string promptFeature,
        string toolsJson,
        string budgetJson,
        DateTimeOffset nowUtc)
        : base(id)
    {
        AgentKey = agentKey;
        DisplayName = displayName;
        CapabilitiesJson = capabilitiesJson;
        PromptFeature = promptFeature;
        ToolsJson = toolsJson;
        BudgetJson = budgetJson;
        Status = AgentStatus.Enabled;
        CreatedAtUtc = nowUtc;
    }

    private AgentRegistration()
    {
    }

    /// <summary>Stable slug identifier used by the Workflow Engine to select agents by capability.
    /// Unique across the platform (e.g. <c>"writing-examiner"</c>, <c>"band-predictor"</c>).</summary>
    public string AgentKey { get; private init; } = string.Empty;

    /// <summary>Human-readable display name for ops/governance dashboards.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Structured JSON blob describing what inputs/outputs the agent accepts.
    /// Kept as opaque text in the domain — the schema is documented in the seeder but not validated
    /// here (same pattern as <c>AiPrompt.SystemTemplate</c> and <c>DailyPlan.PlanItemsJson</c>).</summary>
    public string CapabilitiesJson { get; private set; } = string.Empty;

    /// <summary>The <c>AiPrompt.Feature</c> tag this agent's prompts live under (links into the
    /// <c>AiPrompt</c> catalog without a FK, same as every feature constant in <c>AiFeatures</c>).</summary>
    public string PromptFeature { get; private set; } = string.Empty;

    /// <summary>Structured JSON blob describing the external tools/APIs this agent may call.
    /// Kept as opaque text — analogous to <c>CapabilitiesJson</c>.</summary>
    public string ToolsJson { get; private set; } = string.Empty;

    /// <summary>Structured JSON blob describing the token/cost budget ceiling for this agent's
    /// invocations. Informational — enforcement is via the AI Gateway's existing quota plumbing.</summary>
    public string BudgetJson { get; private set; } = string.Empty;

    /// <summary>Current lifecycle status. <see cref="AgentStatus.Deprecated"/> agents remain
    /// queryable for governance/audit but must not be selected for new workflow invocations.</summary>
    public AgentStatus Status { get; private set; }

    /// <summary>Creates a new enabled <see cref="AgentRegistration"/>.</summary>
    public static Result<AgentRegistration> Register(
        AgentRegistrationId id,
        string agentKey,
        string displayName,
        string capabilitiesJson,
        string promptFeature,
        string toolsJson,
        string budgetJson,
        DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(agentKey) || agentKey.Length > AgentKeyMaxLength)
        {
            return Result.Failure<AgentRegistration>(AgentRegistrationDomainErrors.InvalidAgentKey);
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > DisplayNameMaxLength)
        {
            return Result.Failure<AgentRegistration>(AgentRegistrationDomainErrors.InvalidDisplayName);
        }

        return Result.Success(new AgentRegistration(id, agentKey, displayName, capabilitiesJson, promptFeature, toolsJson, budgetJson, nowUtc));
    }

    /// <summary>Marks the agent deprecated. Idempotent if already deprecated.</summary>
    public void Deprecate(DateTimeOffset nowUtc)
    {
        if (Status == AgentStatus.Deprecated)
        {
            return;
        }

        Status = AgentStatus.Deprecated;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Re-enables a previously deprecated agent. Idempotent if already enabled.</summary>
    public void Enable(DateTimeOffset nowUtc)
    {
        if (Status == AgentStatus.Enabled)
        {
            return;
        }

        Status = AgentStatus.Enabled;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Updates the mutable descriptor fields (capabilities, tools, budget) without changing
    /// the key or status — used by the seeder on subsequent startups to keep metadata current.</summary>
    public void UpdateDescriptors(string displayName, string capabilitiesJson, string promptFeature, string toolsJson, string budgetJson, DateTimeOffset nowUtc)
    {
        DisplayName = displayName;
        CapabilitiesJson = capabilitiesJson;
        PromptFeature = promptFeature;
        ToolsJson = toolsJson;
        BudgetJson = budgetJson;
        UpdatedAtUtc = nowUtc;
    }
}

internal static class AgentRegistrationDomainErrors
{
    public static readonly Error InvalidAgentKey = Error.Validation(
        "VALIDATION_AGENT_REGISTRATION_INVALID_KEY",
        $"AgentKey must be a non-empty string of at most {AgentRegistration.AgentKeyMaxLength} characters.");

    public static readonly Error InvalidDisplayName = Error.Validation(
        "VALIDATION_AGENT_REGISTRATION_INVALID_DISPLAY_NAME",
        $"DisplayName must be a non-empty string of at most {AgentRegistration.DisplayNameMaxLength} characters.");
}
