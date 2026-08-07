using IELTSLms.Abstractions.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.AI.Jobs;

public sealed class AskTutorJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed record Fixture(
        FakeAiOperationRepository Operations, FakeAiProviderRepository Providers, FakeAiPromptRepository Prompts,
        FakeAiResponseRepository Responses, FakeConversationRepository Conversations, FakeConversationMessageRepository Messages,
        FakeUnitOfWork UnitOfWork, FakeAiGateway Gateway, FakeTenantContextAccessor TenantContextAccessor, AskTutorJob Job,
        Guid TenantId, Guid UserId, AiOperation Operation, Conversation Conversation);

    private static Fixture CreateFixture(bool seedPrompt = true)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var operations = new FakeAiOperationRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var conversations = new FakeConversationRepository();
        var messages = new FakeConversationMessageRepository();
        var summaries = new FakeConversationSummaryRepository();
        var unitOfWork = new FakeUnitOfWork();
        var gateway = new FakeAiGateway();
        var tenantContextAccessor = new FakeTenantContextAccessor();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        if (seedPrompt)
        {
            var prompt = AiPrompt.Create(
                AiPromptId.From(Guid.NewGuid()), null, provider.Id, "ai-tutor", 1, "default", "You are an IELTS tutor.", Now).Value;
            prompt.Activate(Now);
            prompts.Seed(prompt);
        }

        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), tenantId, userId, "ai-tutor", Now).Value;
        conversations.Seed(conversation);

        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), tenantId, userId, "ai-tutor", "What is a gerund?", Now);
        operations.Seed(operation);

        var conversationContext = new ConversationContextService(
            conversations, messages, summaries, prompts, providers, responses, gateway, new PromptCompositionService(),
            unitOfWork, new FakeClock(Now), new FakeGuidProvider());

        var job = new AskTutorJob(
            operations, providers, prompts, responses, conversations, messages, unitOfWork, gateway, conversationContext,
            tenantContextAccessor, new FakeClock(Now.AddSeconds(1)), new FakeGuidProvider(), NullLogger<AskTutorJob>.Instance);

        return new Fixture(
            operations, providers, prompts, responses, conversations, messages, unitOfWork, gateway, tenantContextAccessor, job,
            tenantId, userId, operation, conversation);
    }

    [Fact]
    public async Task Schema_valid_output_succeeds_and_appends_the_user_and_assistant_turns()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"answer":"A gerund is the -ing form of a verb used as a noun.","inScope":true}""",
            "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", FromCache: false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Conversation.Id.Value, fixture.UserId, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        Assert.NotNull(fixture.Operation.AiResponseId);
        Assert.Single(fixture.Responses.Responses);

        var history = await fixture.Messages.ListSinceSequenceAsync(fixture.TenantId, fixture.Conversation.Id, 0);
        Assert.Equal(2, history.Count);
        Assert.Equal(ConversationMessageRole.User, history[0].Role);
        Assert.Equal("What is a gerund?", history[0].Content);
        Assert.Equal(ConversationMessageRole.Assistant, history[1].Role);
        Assert.Equal("A gerund is the -ing form of a verb used as a noun.", history[1].Content);
        Assert.Equal(fixture.Operation.AiResponseId, history[1].AiResponseId);
        Assert.Equal(fixture.TenantId, fixture.TenantContextAccessor.TenantId);
    }

    [Fact]
    public async Task Malformed_structured_output_is_recorded_as_invalid_and_fails_the_operation_without_appending_messages()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            "not json", "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Conversation.Id.Value, fixture.UserId, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        var response = Assert.Single(fixture.Responses.Responses);
        Assert.Equal(AiResponseStatus.Invalid, response.Status);
        var history = await fixture.Messages.ListSinceSequenceAsync(fixture.TenantId, fixture.Conversation.Id, 0);
        Assert.Empty(history);
    }

    [Fact]
    public async Task Provider_outage_fails_the_operation_without_writing_an_AiResponse()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Failure<AiGatewayInvocationResult>(
            Error.ServiceUnavailable("DEPENDENCY_AI_UNAVAILABLE", "The AI provider is temporarily unavailable."));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Conversation.Id.Value, fixture.UserId, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Empty(fixture.Responses.Responses);
    }

    [Fact]
    public async Task Already_terminal_operation_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        fixture.Operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Conversation.Id.Value, fixture.UserId, CancellationToken.None);

        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Missing_prompt_configuration_fails_gracefully_without_calling_the_gateway()
    {
        var fixture = CreateFixture(seedPrompt: false);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Operation.Id.Value, fixture.Conversation.Id.Value, fixture.UserId, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal("DEPENDENCY_AI_PROMPT_NOT_CONFIGURED", fixture.Operation.ErrorCode);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }
}
