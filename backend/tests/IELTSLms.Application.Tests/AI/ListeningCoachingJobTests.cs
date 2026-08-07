using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.AI;

public sealed class ListeningCoachingJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed record Fixture(
        FakeAiOperationRepository Operations, FakeAiProviderRepository Providers, FakeAiPromptRepository Prompts,
        FakeAiResponseRepository Responses, FakeUnitOfWork UnitOfWork, FakeAiGateway Gateway,
        FakeTenantContextAccessor TenantContextAccessor, ListeningCoachingJob Job, Guid TenantId, AiOperation Operation);

    private static Fixture CreateFixture()
    {
        var tenantId = Guid.NewGuid();
        var operations = new FakeAiOperationRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var gateway = new FakeAiGateway();
        var tenantContextAccessor = new FakeTenantContextAccessor();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, provider.Id, "listening-coach", 1, "default", "system template", Now).Value;
        prompt.Activate(Now);
        prompts.Seed(prompt);

        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), "listening-coach", "Audio transcript: ...", Now);
        operations.Seed(operation);

        var job = new ListeningCoachingJob(
            operations, providers, prompts, responses, unitOfWork, gateway, new PromptCompositionService(),
            tenantContextAccessor, new FakeClock(Now.AddSeconds(1)), new FakeGuidProvider(), NullLogger<ListeningCoachingJob>.Instance);

        return new Fixture(operations, providers, prompts, responses, unitOfWork, gateway, tenantContextAccessor, job, tenantId, operation);
    }

    [Fact]
    public async Task Schema_valid_output_succeeds_the_operation_and_writes_an_AiResponse()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new IELTSLms.Abstractions.AI.AiGatewayInvocationResult(
            """{"explanation":"The library closes at nine, not seven.","strategyTips":["Listen for the exact number, distractors mention nearby ones."]}""",
            "claude-3-5-haiku", 12, 8, 120, 0, "USD", "msg_abc", FromCache: false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        Assert.NotNull(fixture.Operation.AiResponseId);
        var response = Assert.Single(fixture.Responses.Responses);
        Assert.Equal(AiResponseStatus.Succeeded, response.Status);
        Assert.Equal(fixture.TenantId, fixture.TenantContextAccessor.TenantId);
    }

    [Fact]
    public async Task Malformed_structured_output_is_recorded_as_invalid_and_fails_the_operation()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new IELTSLms.Abstractions.AI.AiGatewayInvocationResult(
            "not json at all", "claude-3-5-haiku", 12, 8, 120, 0, "USD", "msg_abc", FromCache: false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        var response = Assert.Single(fixture.Responses.Responses);
        Assert.Equal(AiResponseStatus.Invalid, response.Status);
        Assert.Null(response.OutputJson);
    }

    [Fact]
    public async Task Already_terminal_operation_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        fixture.Operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, CancellationToken.None);

        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }
}
