using IELTSLms.Domain.AI;

namespace IELTSLms.Application.AI.Orchestration;

/// <summary>
/// Read-only service layer over the <see cref="AgentRegistration"/> catalog (Sprint 10, Agent Registry).
/// Provides the governance/discovery view of all registered agents. Access is still gated by the
/// existing <c>Features</c>/permission checks — the registry is a catalog, not an access-control list.
/// </summary>
public interface IAgentRegistry
{
    /// <summary>Returns all registered agents (enabled and deprecated), ordered by AgentKey.
    /// Powers the <c>GET /api/v1/ai/agents</c> governance endpoint.</summary>
    Task<IReadOnlyList<AgentRegistrationResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single agent by its stable key slug, or null if not registered.
    /// Used by the Workflow Engine to resolve agent metadata before execution.</summary>
    Task<AgentRegistrationResponse?> GetByKeyAsync(string agentKey, CancellationToken cancellationToken = default);
}

/// <summary>Read-only projection of an <see cref="AgentRegistration"/> for API responses.</summary>
public sealed record AgentRegistrationResponse(
    Guid Id,
    string AgentKey,
    string DisplayName,
    string CapabilitiesJson,
    string PromptFeature,
    string ToolsJson,
    string BudgetJson,
    string Status,
    DateTimeOffset RegisteredAt);
