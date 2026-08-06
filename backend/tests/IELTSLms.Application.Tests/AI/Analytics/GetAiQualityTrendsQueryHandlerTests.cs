using IELTSLms.Application.AI.GetAiQualityTrends;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;
using Shouldly;

namespace IELTSLms.Application.Tests.AI.Analytics;

public class GetAiQualityTrendsQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ShouldReturnQualityTrends()
    {
        // Arrange
        var repos = new FakeAiResponseRepository();
        
        var now = DateTimeOffset.UtcNow;
        var r1 = AiResponse.Create(
            AiResponseId.From(Guid.NewGuid()), TenantId, AiProviderId.From(Guid.NewGuid()), null, 1,
            "mock", Guid.NewGuid(), "hash", "model", 10, 20, 100, 30L, "USD", AiResponseStatus.Failed, null, null, now).Value;
        repos.Seed(r1, "grammar", "openai");
        
        var r2 = AiResponse.Create(
            AiResponseId.From(Guid.NewGuid()), TenantId, AiProviderId.From(Guid.NewGuid()), null, 1,
            "mock", Guid.NewGuid(), "hash", "model", 10, 20, 100, 30L, "USD", AiResponseStatus.Succeeded, "{}", "pid", now).Value;
        repos.Seed(r2, "grammar", "openai");

        var handler = new GetAiQualityTrendsQueryHandler(repos);
        var query = new GetAiQualityTrendsQuery(TenantId, now.AddDays(-1), now.AddDays(1));

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].Capability.ShouldBe("grammar");
        result.Value[0].TotalRequests.ShouldBe(2);
        result.Value[0].FailedRequests.ShouldBe(1);
        result.Value[0].FailureRate.ShouldBe(0.5m);
    }
}
