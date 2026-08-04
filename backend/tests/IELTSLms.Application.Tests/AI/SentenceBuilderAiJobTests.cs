using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.SentenceBuilder.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.AI;

/// <summary>Contract test per Sprint 7's testing section: AI enrichment populates
/// <c>AiResponseId</c>/<c>Feedback</c> asynchronously without touching the synchronous score.</summary>
public sealed class SentenceBuilderAiJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed record Fixture(
        FakeSentenceAttemptRepository Attempts, FakeAiResponseRepository Responses, FakeAiGateway Gateway,
        SentenceBuilderAiJob Job, Guid TenantId, SentenceAttempt Attempt);

    private static Fixture CreateFixture()
    {
        var tenantId = Guid.NewGuid();
        var exercises = new FakeSentenceExerciseRepository();
        var attempts = new FakeSentenceAttemptRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var gateway = new FakeAiGateway();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, provider.Id, "sentence-builder-ai", 1, "default", "system template", Now).Value;
        prompt.Activate(Now);
        prompts.Seed(prompt);

        var exercise = SentenceExercise.Create(
            SentenceExerciseId.From(Guid.NewGuid()), tenantId, "Write a concessive sentence.", """["although"]""",
            SentenceDifficulty.Intermediate, Now).Value;
        exercises.Seed(exercise);

        var attempt = SentenceAttempt.Submit(
            SentenceAttemptId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), exercise.Id, "It was raining.", 0, Now);
        attempts.Seed(attempt);

        var job = new SentenceBuilderAiJob(
            attempts, exercises, providers, prompts, responses, new FakeUnitOfWork(), gateway, new PromptCompositionService(),
            new FakeTenantContextAccessor(), new FakeClock(Now.AddSeconds(1)), new FakeGuidProvider(),
            NullLogger<SentenceBuilderAiJob>.Instance);

        return new Fixture(attempts, responses, gateway, job, tenantId, attempt);
    }

    [Fact]
    public async Task Schema_valid_output_attaches_feedback_without_changing_the_deterministic_score()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new IELTSLms.Abstractions.AI.AiGatewayInvocationResult(
            """{"matchesPattern":false,"feedback":"Missing the concessive clause.","suggestedRevision":"Although it was raining, she went out."}""",
            "claude-3-5-haiku", 12, 8, 120, 0, "USD", "msg_abc", FromCache: false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(0, fixture.Attempt.Score);
        Assert.NotNull(fixture.Attempt.AiResponseId);
        Assert.Contains("Missing the concessive clause.", fixture.Attempt.Feedback);
        Assert.Contains("Although it was raining, she went out.", fixture.Attempt.Feedback);
        Assert.Single(fixture.Responses.Responses);
    }

    [Fact]
    public async Task Malformed_structured_output_leaves_the_attempt_without_feedback_rather_than_throwing()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new IELTSLms.Abstractions.AI.AiGatewayInvocationResult(
            "not json at all", "claude-3-5-haiku", 12, 8, 120, 0, "USD", "msg_abc", FromCache: false));

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Null(fixture.Attempt.AiResponseId);
        Assert.Null(fixture.Attempt.Feedback);
        var response = Assert.Single(fixture.Responses.Responses);
        Assert.Equal(AiResponseStatus.Invalid, response.Status);
    }

    [Fact]
    public async Task Already_enriched_attempt_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        fixture.Attempt.AttachAiFeedback(AiResponseId.From(Guid.NewGuid()), "Already done.", Now);

        await fixture.Job.ExecuteAsync(fixture.TenantId, fixture.Attempt.Id.Value, CancellationToken.None);

        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }
}
