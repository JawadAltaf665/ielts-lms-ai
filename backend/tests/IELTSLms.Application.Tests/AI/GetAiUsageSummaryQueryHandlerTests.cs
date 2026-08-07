using IELTSLms.Application.AI.GetAiUsageSummary;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI;

public sealed class GetAiUsageSummaryQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);

    private static AiResponse CreateResponse(Guid tenantId, DateTimeOffset createdAtUtc) =>
        AiResponse.Create(
            AiResponseId.From(Guid.NewGuid()), tenantId, AiProviderId.From(Guid.NewGuid()), AiPromptId.From(Guid.NewGuid()),
            1, nameof(AiOperation), Guid.NewGuid(), "hash", "claude-3-5-haiku", 100, 50, 120, 10, "USD",
            AiResponseStatus.Succeeded, "{}", "msg_1", createdAtUtc).Value;

    [Fact]
    public async Task Returns_correctly_grouped_totals_scoped_to_the_requesting_tenant()
    {
        var responses = new FakeAiResponseRepository();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        responses.Seed(CreateResponse(tenantId, Now.AddDays(-1)), "grammar-coach", "claude");
        responses.Seed(CreateResponse(tenantId, Now.AddDays(-1)), "grammar-coach", "claude");
        responses.Seed(CreateResponse(tenantId, Now.AddDays(-2)), "writing-examiner", "claude");
        responses.Seed(CreateResponse(otherTenantId, Now.AddDays(-1)), "grammar-coach", "claude");

        var handler = new GetAiUsageSummaryQueryHandler(responses, new FakeClock(Now));

        var result = await handler.Handle(new GetAiUsageSummaryQuery(tenantId, From: null, To: null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);

        var grammarBucket = Assert.Single(result.Value.Items, i => i.Capability == "grammar-coach");
        Assert.Equal(2, grammarBucket.RequestCount);
        Assert.Equal(200, grammarBucket.TotalPromptTokens);
        Assert.Equal(20, grammarBucket.TotalCostMinorUnits);

        var writingBucket = Assert.Single(result.Value.Items, i => i.Capability == "writing-examiner");
        Assert.Equal(1, writingBucket.RequestCount);

        Assert.DoesNotContain(result.Value.Items, i => i.RequestCount == 0);
    }

    [Fact]
    public async Task Never_leaks_another_tenants_rows()
    {
        var responses = new FakeAiResponseRepository();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        responses.Seed(CreateResponse(otherTenantId, Now.AddDays(-1)), "grammar-coach", "claude");

        var handler = new GetAiUsageSummaryQueryHandler(responses, new FakeClock(Now));

        var result = await handler.Handle(new GetAiUsageSummaryQuery(tenantId, From: null, To: null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    [Fact]
    public async Task Invalid_range_is_rejected()
    {
        var handler = new GetAiUsageSummaryQueryHandler(new FakeAiResponseRepository(), new FakeClock(Now));

        var result = await handler.Handle(
            new GetAiUsageSummaryQuery(Guid.NewGuid(), From: DateOnly.FromDateTime(Now.UtcDateTime), To: DateOnly.FromDateTime(Now.AddDays(-5).UtcDateTime)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_AI_USAGE_SUMMARY_INVALID_RANGE", result.Error.Code);
    }
}
