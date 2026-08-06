using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Application.AI.Orchestration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace IELTSLms.Integration.Tests.AI;

public class CrossAiModuleIntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public CrossAiModuleIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void AiModule_ShouldRegister_WorkflowEngine_And_AgentRegistry()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act
        var engine = scope.ServiceProvider.GetService<IWorkflowEngine>();
        var registry = scope.ServiceProvider.GetService<IAgentRegistry>();
        var agentRepo = scope.ServiceProvider.GetService<IAgentRegistrationRepository>();
        var workflowRunRepo = scope.ServiceProvider.GetService<IWorkflowRunRepository>();

        // Assert
        engine.ShouldNotBeNull();
        registry.ShouldNotBeNull();
        agentRepo.ShouldNotBeNull();
        workflowRunRepo.ShouldNotBeNull();
    }
}
