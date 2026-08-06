using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.AI;

namespace IELTSLms.Abstractions.Persistence.AI;

/// <summary>
/// Aggregate access for <see cref="AgentRegistration"/> (Sprint 10, Agent Registry).
/// Global (not tenant-scoped) — queries do not filter by tenant. Backed by
/// <c>AgentRegistrationRepository</c> in Persistence.
/// </summary>
public interface IAgentRegistrationRepository : IRepository<AgentRegistration, AgentRegistrationId>
{
    /// <summary>Returns all registered agents (enabled and deprecated), ordered by AgentKey.
    /// Used by the <c>GET /api/v1/ai/agents</c> governance endpoint.</summary>
    Task<IReadOnlyList<AgentRegistration>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the agent registration for the given key, or null if not registered.
    /// Used by the Workflow Engine to verify an agent is registered before invoking it.</summary>
    Task<AgentRegistration?> GetByKeyAsync(string agentKey, CancellationToken cancellationToken = default);
}
