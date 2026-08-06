using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAgentRegistrationRepository : IAgentRegistrationRepository
{
    private readonly List<AgentRegistration> _agents = [];

    public IReadOnlyList<AgentRegistration> Agents => _agents;

    public void Seed(AgentRegistration agent) => _agents.Add(agent);

    public Task<IReadOnlyList<AgentRegistration>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AgentRegistration>>(_agents.OrderBy(a => a.AgentKey).ToList());

    public Task<AgentRegistration?> GetByKeyAsync(string agentKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(_agents.FirstOrDefault(a => a.AgentKey == agentKey));

    public Task<AgentRegistration?> GetByIdAsync(AgentRegistrationId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_agents.FirstOrDefault(a => a.Id == id));

    public void Add(AgentRegistration aggregate) => _agents.Add(aggregate);

    public void Remove(AgentRegistration aggregate) => _agents.Remove(aggregate);
}
