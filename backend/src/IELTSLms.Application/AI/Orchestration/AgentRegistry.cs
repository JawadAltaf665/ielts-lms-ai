using IELTSLms.Abstractions.Persistence.AI;

namespace IELTSLms.Application.AI.Orchestration;

/// <summary>
/// Application-layer implementation of <see cref="IAgentRegistry"/> (Sprint 10).
/// Thin read-through service over <see cref="IAgentRegistrationRepository"/> — no caching needed
/// because this is an admin/governance read path, not the hot learner path.
/// </summary>
public sealed class AgentRegistry(IAgentRegistrationRepository repository) : IAgentRegistry
{
    public async Task<IReadOnlyList<AgentRegistrationResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var agents = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return agents.Select(ToResponse).ToList();
    }

    public async Task<AgentRegistrationResponse?> GetByKeyAsync(string agentKey, CancellationToken cancellationToken = default)
    {
        var agent = await repository.GetByKeyAsync(agentKey, cancellationToken).ConfigureAwait(false);
        return agent is null ? null : ToResponse(agent);
    }

    private static AgentRegistrationResponse ToResponse(Domain.AI.AgentRegistration agent) =>
        new(agent.Id.Value, agent.AgentKey, agent.DisplayName, agent.CapabilitiesJson,
            agent.PromptFeature, agent.ToolsJson, agent.BudgetJson,
            agent.Status.ToString(), agent.CreatedAtUtc);
}
