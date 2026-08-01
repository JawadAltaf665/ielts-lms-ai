using IELTSLms.Abstractions.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI;

/// <summary>Exhaustive context-assembly budget tests per the Phase 7 Sprint 3 acceptance criteria: empty
/// history, history under budget, history over budget triggering summarization, and summary-plus-tail
/// composition.</summary>
public sealed class ConversationContextServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Dictionary<string, string> NoVariables = [];

    private sealed record Fixture(
        FakeConversationRepository Conversations, FakeConversationMessageRepository Messages,
        FakeConversationSummaryRepository Summaries, FakeAiPromptRepository Prompts, FakeAiProviderRepository Providers,
        FakeAiResponseRepository Responses, FakeAiGateway Gateway, FakeUnitOfWork UnitOfWork,
        ConversationContextService Service, Guid TenantId, Guid UserId, AiPrompt CapabilityPrompt);

    private static Fixture CreateFixture(bool seedSummaryPrompt = true)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var conversations = new FakeConversationRepository();
        var messages = new FakeConversationMessageRepository();
        var summaries = new FakeConversationSummaryRepository();
        var prompts = new FakeAiPromptRepository();
        var providers = new FakeAiProviderRepository();
        var responses = new FakeAiResponseRepository();
        var gateway = new FakeAiGateway();
        var unitOfWork = new FakeUnitOfWork();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        var capabilityPrompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), tenantId, provider.Id, "ai-tutor", 1, "default", "You are a coach.", Now).Value;
        capabilityPrompt.Activate(Now);

        if (seedSummaryPrompt)
        {
            var summaryPrompt = AiPrompt.Create(
                AiPromptId.From(Guid.NewGuid()), null, provider.Id, "conversation-summary", 1, "default",
                "Summarize the turns.", Now).Value;
            summaryPrompt.Activate(Now);
            prompts.Seed(summaryPrompt);
        }

        var service = new ConversationContextService(
            conversations, messages, summaries, prompts, providers, responses, gateway, new PromptCompositionService(),
            unitOfWork, new FakeClock(Now.AddDays(1)), new FakeGuidProvider());

        return new Fixture(conversations, messages, summaries, prompts, providers, responses, gateway, unitOfWork, service, tenantId, userId, capabilityPrompt);
    }

    private static ConversationMessage SeedMessage(
        FakeConversationMessageRepository repo, Guid tenantId, ConversationId conversationId, int sequence, int tokenCount)
    {
        var message = ConversationMessage.Create(
            ConversationMessageId.From(Guid.NewGuid()), tenantId, conversationId, ConversationMessageRole.User,
            $"turn {sequence}", sequence, tokenCount, null, Now).Value;
        repo.Seed(message);
        return message;
    }

    [Fact]
    public async Task Empty_history_composes_the_prompt_with_no_prior_turns()
    {
        var fixture = CreateFixture();
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, "ai-tutor", Now).Value;
        fixture.Conversations.Seed(conversation);

        var result = await fixture.Service.AssembleAsync(
            fixture.TenantId, fixture.UserId, conversation.Id, fixture.CapabilityPrompt,
            NoVariables, "Hi coach", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("You are a coach.", result.Value.SystemPrompt);
        Assert.Equal("Hi coach", result.Value.UserContent);
        Assert.Empty(result.Value.History);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task History_under_budget_is_returned_raw_without_summarizing()
    {
        var fixture = CreateFixture();
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, "ai-tutor", Now).Value;
        fixture.Conversations.Seed(conversation);
        for (var sequence = 1; sequence <= 3; sequence++)
        {
            SeedMessage(fixture.Messages, fixture.TenantId, conversation.Id, sequence, tokenCount: 10);
        }

        var result = await fixture.Service.AssembleAsync(
            fixture.TenantId, fixture.UserId, conversation.Id, fixture.CapabilityPrompt, NoVariables, "next turn", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.History.Count);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
        var summary = await fixture.Summaries.GetLatestAsync(fixture.TenantId, conversation.Id, CancellationToken.None);
        Assert.Null(summary);
    }

    [Fact]
    public async Task History_over_budget_triggers_summarization_and_persists_exactly_one_summary()
    {
        var fixture = CreateFixture();
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, "ai-tutor", Now).Value;
        fixture.Conversations.Seed(conversation);
        // 10 messages x 300 tokens = 3000 > the 2000 budget, and 10 > the 6-message recency floor.
        for (var sequence = 1; sequence <= 10; sequence++)
        {
            SeedMessage(fixture.Messages, fixture.TenantId, conversation.Id, sequence, tokenCount: 300);
        }
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"summary":"Learner practiced ten turns of Task 2 essays."}""", "claude-3-5-haiku", 50, 20, 90, 0, "USD", "msg_sum", FromCache: false));

        var result = await fixture.Service.AssembleAsync(
            fixture.TenantId, fixture.UserId, conversation.Id, fixture.CapabilityPrompt, NoVariables, "next turn", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.Gateway.InvokeCallCount);
        var response = Assert.Single(fixture.Responses.Responses);
        Assert.Equal(AiResponseStatus.Succeeded, response.Status);
        var persistedSummary = await fixture.Summaries.GetLatestAsync(fixture.TenantId, conversation.Id, CancellationToken.None);
        Assert.NotNull(persistedSummary);
        // Recency floor: the 6 most recent messages (sequences 5-10) are never compacted away.
        Assert.Equal(4, persistedSummary!.CoversUpToSequence);
    }

    [Fact]
    public async Task Summary_plus_tail_composition_puts_the_summary_first_as_a_system_turn()
    {
        var fixture = CreateFixture();
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, "ai-tutor", Now).Value;
        fixture.Conversations.Seed(conversation);
        var summary = ConversationSummary.Create(
            ConversationSummaryId.From(Guid.NewGuid()), fixture.TenantId, conversation.Id, "Prior turns summarized.", 4, Now).Value;
        fixture.Summaries.Seed(summary);
        for (var sequence = 5; sequence <= 7; sequence++)
        {
            SeedMessage(fixture.Messages, fixture.TenantId, conversation.Id, sequence, tokenCount: 10);
        }

        var result = await fixture.Service.AssembleAsync(
            fixture.TenantId, fixture.UserId, conversation.Id, fixture.CapabilityPrompt, NoVariables, "next turn", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.History.Count);
        Assert.Equal(ConversationMessageRole.System, result.Value.History[0].Role);
        Assert.Equal("Prior turns summarized.", result.Value.History[0].Content);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Missing_summarization_prompt_fails_gracefully_when_summarization_is_needed()
    {
        var fixture = CreateFixture(seedSummaryPrompt: false);
        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, "ai-tutor", Now).Value;
        fixture.Conversations.Seed(conversation);
        for (var sequence = 1; sequence <= 10; sequence++)
        {
            SeedMessage(fixture.Messages, fixture.TenantId, conversation.Id, sequence, tokenCount: 300);
        }

        var result = await fixture.Service.AssembleAsync(
            fixture.TenantId, fixture.UserId, conversation.Id, fixture.CapabilityPrompt, NoVariables, "next turn", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("DEPENDENCY_AI_CONVERSATION_SUMMARY_PROMPT_NOT_CONFIGURED", result.Error.Code);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
        Assert.Empty(fixture.Responses.Responses);
    }

    [Fact]
    public async Task Conversation_owned_by_another_user_is_not_found()
    {
        var fixture = CreateFixture();
        var foreign = Conversation.Create(ConversationId.From(Guid.NewGuid()), fixture.TenantId, Guid.NewGuid(), "ai-tutor", Now).Value;
        fixture.Conversations.Seed(foreign);

        var result = await fixture.Service.AssembleAsync(
            fixture.TenantId, fixture.UserId, foreign.Id, fixture.CapabilityPrompt, NoVariables, "hi", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_CONVERSATION", result.Error.Code);
    }
}
