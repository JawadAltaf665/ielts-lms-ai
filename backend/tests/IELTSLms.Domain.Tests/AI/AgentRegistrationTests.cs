using IELTSLms.Domain.AI;
using Shouldly;

namespace IELTSLms.Domain.Tests.AI;

public class AgentRegistrationTests
{
    [Fact]
    public void Register_ShouldCreateValidAgentRegistration()
    {
        // Arrange
        var id = AgentRegistrationId.From(Guid.NewGuid());
        var agentKey = "test-agent";
        var displayName = "Test Agent";
        var capabilitiesJson = "[\"chat\"]";
        var promptFeature = "ai.test.use";
        var toolsJson = "[]";
        var budgetJson = "{}";
        var now = DateTimeOffset.UtcNow;

        // Act
        var result = AgentRegistration.Register(id, agentKey, displayName, capabilitiesJson, promptFeature, toolsJson, budgetJson, now);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var agent = result.Value;
        agent.Id.ShouldBe(id);
        agent.AgentKey.ShouldBe(agentKey);
        agent.DisplayName.ShouldBe(displayName);
        agent.CapabilitiesJson.ShouldBe(capabilitiesJson);
        agent.PromptFeature.ShouldBe(promptFeature);
        agent.ToolsJson.ShouldBe(toolsJson);
        agent.BudgetJson.ShouldBe(budgetJson);
        agent.Status.ShouldBe(AgentStatus.Enabled);
        agent.CreatedAtUtc.ShouldBe(now);
    }

    [Fact]
    public void Update_ShouldUpdateMetadataAndRecordTimestamp()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var agent = AgentRegistration.Register(
            AgentRegistrationId.From(Guid.NewGuid()), "test-agent", "Test", "[]", "feature", "[]", "{}", now).Value;

        var updateTime = now.AddHours(1);

        // Act
        agent.UpdateDescriptors("Updated Name", "[\"read\"]", "updated.feature", "[{}]", "{\"cost\":1}", updateTime);

        // Assert
        agent.DisplayName.ShouldBe("Updated Name");
        agent.CapabilitiesJson.ShouldBe("[\"read\"]");
        agent.PromptFeature.ShouldBe("updated.feature");
        agent.ToolsJson.ShouldBe("[{}]");
        agent.BudgetJson.ShouldBe("{\"cost\":1}");
        agent.UpdatedAtUtc.ShouldBe(updateTime);
    }

    [Fact]
    public void Deprecate_ShouldChangeStatusToDeprecated()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var agent = AgentRegistration.Register(
            AgentRegistrationId.From(Guid.NewGuid()), "test-agent", "Test", "[]", "feature", "[]", "{}", now).Value;

        var deprecateTime = now.AddHours(1);

        // Act
        agent.Deprecate(deprecateTime);

        // Assert
        agent.Status.ShouldBe(AgentStatus.Deprecated);
        agent.UpdatedAtUtc.ShouldBe(deprecateTime);
    }
}
