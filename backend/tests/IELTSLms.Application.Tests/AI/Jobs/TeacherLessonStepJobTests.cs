using IELTSLms.Abstractions.AI;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.AI.Events;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.AI.Jobs;

/// <summary>Golden-set coverage per lesson-step type (Explain/CheckUnderstanding/Practice/Recap) plus
/// the state-machine invariants the sprint's Testing section calls out: no step-skipping, no advance on
/// a failed check, exactly-once completion, and idempotency under a retried operation.</summary>
public sealed class TeacherLessonStepJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed record Fixture(
        FakeAiOperationRepository Operations, FakeAiPromptRepository Prompts, FakeAiResponseRepository Responses,
        FakeLessonSessionRepository LessonSessions, FakeConversationRepository Conversations, FakeConversationMessageRepository Messages,
        FakeAiGateway Gateway, TeacherLessonStepJob Job, Guid TenantId, Guid UserId, LessonSession Session);

    private static Fixture CreateFixture(bool seedAllPrompts = true)
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var operations = new FakeAiOperationRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var lessonSessions = new FakeLessonSessionRepository();
        var conversations = new FakeConversationRepository();
        var messages = new FakeConversationMessageRepository();
        var summaries = new FakeConversationSummaryRepository();
        var unitOfWork = new FakeUnitOfWork();
        var gateway = new FakeAiGateway();
        var tenantContextAccessor = new FakeTenantContextAccessor();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        if (seedAllPrompts)
        {
            foreach (var scope in new[] { "explain", "check-understanding", "practice", "recap" })
            {
                var prompt = AiPrompt.Create(
                    AiPromptId.From(Guid.NewGuid()), null, provider.Id, "ai-teacher", 1, scope, "You are an IELTS teacher.", Now).Value;
                prompt.Activate(Now);
                prompts.Seed(prompt);
            }
        }

        var conversation = Conversation.Create(ConversationId.From(Guid.NewGuid()), tenantId, userId, "ai-teacher", Now).Value;
        conversations.Seed(conversation);

        var session = LessonSession.Start(LessonSessionId.From(Guid.NewGuid()), tenantId, userId, Guid.NewGuid(), conversation.Id, Now);
        lessonSessions.Seed(session);

        var conversationContext = new ConversationContextService(
            conversations, messages, summaries, prompts, providers, responses, gateway, new PromptCompositionService(),
            unitOfWork, new FakeClock(Now), new FakeGuidProvider());

        var job = new TeacherLessonStepJob(
            operations, providers, prompts, responses, lessonSessions, conversations, messages, unitOfWork, gateway, conversationContext,
            tenantContextAccessor, new FakeClock(Now.AddSeconds(1)), new FakeGuidProvider(), NullLogger<TeacherLessonStepJob>.Instance);

        return new Fixture(operations, prompts, responses, lessonSessions, conversations, messages, gateway, job, tenantId, userId, session);
    }

    private static AiOperation SeedOperation(Fixture fixture, string inputText)
    {
        var operation = AiOperation.Enqueue(AiOperationId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, "ai-teacher", inputText, Now);
        fixture.Operations.Seed(operation);
        return operation;
    }

    [Fact]
    public async Task Session_start_generates_the_explain_content_and_does_not_advance_on_its_own_turn()
    {
        var fixture = CreateFixture();
        var operation = SeedOperation(fixture, "Conditionals");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"content":"Conditionals describe hypothetical situations."}""", "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: true, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, operation.Status);
        Assert.Equal(LessonStep.Explain, fixture.Session.CurrentStep);
        var history = await fixture.Messages.ListSinceSequenceAsync(fixture.TenantId, fixture.Session.ConversationId, 0);
        var message = Assert.Single(history);
        Assert.Equal(ConversationMessageRole.Assistant, message.Role);
        Assert.Equal("Conditionals describe hypothetical situations.", message.Content);
    }

    [Fact]
    public async Task Responding_to_Explain_appends_both_turns_and_advances_to_CheckUnderstanding()
    {
        var fixture = CreateFixture();
        var operation = SeedOperation(fixture, "OK, I think I understand.");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"content":"Can you use 'if' in a conditional sentence?"}""", "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(LessonStep.CheckUnderstanding, fixture.Session.CurrentStep);
        var history = await fixture.Messages.ListSinceSequenceAsync(fixture.TenantId, fixture.Session.ConversationId, 0);
        Assert.Equal(2, history.Count);
        Assert.Equal(ConversationMessageRole.User, history[0].Role);
        Assert.Equal(ConversationMessageRole.Assistant, history[1].Role);
    }

    [Fact]
    public async Task A_passed_check_advances_to_Practice()
    {
        var fixture = CreateFixture();
        fixture.Session.Advance(Now);
        Assert.Equal(LessonStep.CheckUnderstanding, fixture.Session.CurrentStep);
        var operation = SeedOperation(fixture, "Yes, if it rains I will stay home.");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"content":"Correct!","passed":true}""", "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(LessonStep.Practice, fixture.Session.CurrentStep);
    }

    [Fact]
    public async Task A_failed_check_under_the_retry_cap_repeats_the_step_instead_of_advancing()
    {
        var fixture = CreateFixture();
        fixture.Session.Advance(Now);
        var operation = SeedOperation(fixture, "I don't know.");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"content":"Not quite — try again.","passed":false}""", "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(LessonStep.CheckUnderstanding, fixture.Session.CurrentStep);
        Assert.Equal(1, fixture.Session.CheckAttempts);
        Assert.Equal(AiOperationStatus.Succeeded, operation.Status);
    }

    [Fact]
    public async Task Exhausting_the_retry_cap_force_advances_rather_than_looping_forever()
    {
        var fixture = CreateFixture();
        fixture.Session.Advance(Now);
        fixture.Session.RecordCheckAttempt(passed: false, Now);
        fixture.Session.RecordCheckAttempt(passed: false, Now);
        Assert.Equal(LessonStep.CheckUnderstanding, fixture.Session.CurrentStep);

        var operation = SeedOperation(fixture, "I still don't know.");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"content":"Let's move on.","passed":false}""", "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(LessonStep.Practice, fixture.Session.CurrentStep);
        Assert.Equal(3, fixture.Session.CheckAttempts);
    }

    [Fact]
    public async Task Responding_to_Recap_completes_the_session_exactly_once()
    {
        var fixture = CreateFixture();
        fixture.Session.Advance(Now);
        fixture.Session.RecordCheckAttempt(passed: true, Now);
        fixture.Session.Advance(Now);
        Assert.Equal(LessonStep.Recap, fixture.Session.CurrentStep);

        var operation = SeedOperation(fixture, "Thanks, that was helpful.");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            """{"content":"Great work today!"}""", "claude-3-5-haiku", 20, 15, 150, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(LessonStep.Completed, fixture.Session.CurrentStep);
        Assert.NotNull(fixture.Session.CompletedAtUtc);
        Assert.Single(fixture.Session.DomainEvents.OfType<LessonSessionCompletedDomainEvent>());
    }

    [Fact]
    public async Task Malformed_output_fails_the_operation_without_advancing_the_step_or_appending_messages()
    {
        var fixture = CreateFixture();
        var operation = SeedOperation(fixture, "OK");
        fixture.Gateway.NextResult = Result.Success(new AiGatewayInvocationResult(
            "not json", "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, operation.Status);
        Assert.Equal(LessonStep.Explain, fixture.Session.CurrentStep);
        var history = await fixture.Messages.ListSinceSequenceAsync(fixture.TenantId, fixture.Session.ConversationId, 0);
        Assert.Empty(history);
    }

    [Fact]
    public async Task A_session_already_completed_at_call_time_fails_the_operation_without_calling_the_gateway()
    {
        var fixture = CreateFixture();
        fixture.Session.Advance(Now);
        fixture.Session.RecordCheckAttempt(passed: true, Now);
        fixture.Session.Advance(Now);
        fixture.Session.Advance(Now);
        Assert.Equal(LessonStep.Completed, fixture.Session.CurrentStep);
        var operation = SeedOperation(fixture, "Anything else?");

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: false, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, operation.Status);
        Assert.Equal("BUSINESS_LESSON_SESSION_COMPLETED", operation.ErrorCode);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Missing_step_prompt_fails_gracefully_without_calling_the_gateway()
    {
        var fixture = CreateFixture(seedAllPrompts: false);
        var operation = SeedOperation(fixture, "Conditionals");

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: true, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, operation.Status);
        Assert.Equal("DEPENDENCY_AI_TEACHER_STEP_PROMPT_NOT_CONFIGURED", operation.ErrorCode);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Already_terminal_operation_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        var operation = SeedOperation(fixture, "Conditionals");
        operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now);

        await fixture.Job.ExecuteAsync(fixture.TenantId, operation.Id.Value, fixture.Session.Id.Value, isSessionStart: true, CancellationToken.None);

        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }
}
