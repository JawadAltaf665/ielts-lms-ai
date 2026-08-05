using System.Text.Json;
using IELTSLms.Application.AI;
using IELTSLms.Application.AI.Jobs;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Reading;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;
using FakeReadingQuestionRepository = IELTSLms.Application.Tests.Reading.Fakes.FakeReadingQuestionRepository;

namespace IELTSLms.Application.Tests.AI;

public sealed class GenerateQuestionVariantsJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static string Key(params string[] alternatives) => JsonSerializer.Serialize(alternatives);

    private sealed record Fixture(
        FakeAiOperationRepository Operations, FakeAiProviderRepository Providers, FakeAiPromptRepository Prompts,
        FakeAiResponseRepository Responses, FakeQuestionVariantDraftRepository Drafts, FakeReadingQuestionRepository ReadingQuestions,
        FakeUnitOfWork UnitOfWork, FakeAiGateway Gateway, GenerateQuestionVariantsJob Job, Guid TenantId, AiOperation Operation,
        ReadingQuestion SourceQuestion);

    private static Fixture CreateFixture()
    {
        var tenantId = Guid.NewGuid();
        var operations = new FakeAiOperationRepository();
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var responses = new FakeAiResponseRepository();
        var drafts = new FakeQuestionVariantDraftRepository();
        var readingQuestions = new FakeReadingQuestionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var gateway = new FakeAiGateway();

        var provider = AiProvider.Create(
            AiProviderId.From(Guid.NewGuid()), null, "claude", AiProviderKind.Claude, "https://api.anthropic.com", Now).Value;
        providers.Seed(provider);

        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), null, provider.Id, "reading-question-variants", 1, "default", "system template", Now).Value;
        prompt.Activate(Now);
        prompts.Seed(prompt);

        var sourceQuestion = ReadingQuestion.Create(
            ReadingQuestionId.From(Guid.NewGuid()), tenantId, ReadingPassageId.From(Guid.NewGuid()), ReadingQuestionType.MultipleChoice,
            "What drives ocean currents?", Key("heat"), 0, Now).Value;
        readingQuestions.Seed(sourceQuestion);

        var operation = AiOperation.Enqueue(
            AiOperationId.From(Guid.NewGuid()), tenantId, Guid.NewGuid(), "reading-question-variants", "Passage: ...", Now);
        operations.Seed(operation);

        var job = new GenerateQuestionVariantsJob(
            operations, providers, prompts, responses, drafts, readingQuestions, new Application.Tests.Listening.Fakes.FakeListeningQuestionRepository(),
            unitOfWork, gateway, new PromptCompositionService(), new FakeTenantContextAccessor(), new FakeClock(Now.AddSeconds(1)),
            new FakeGuidProvider(), NullLogger<GenerateQuestionVariantsJob>.Instance);

        return new Fixture(operations, providers, prompts, responses, drafts, readingQuestions, unitOfWork, gateway, job, tenantId, operation, sourceQuestion);
    }

    [Fact]
    public async Task Schema_valid_output_creates_a_pending_draft_sharing_the_operation_id()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new IELTSLms.Abstractions.AI.AiGatewayInvocationResult(
            """{"prompt":"What primarily drives the movement of ocean currents?","answerKey":["heat"]}""",
            "claude-3-5-haiku", 12, 8, 120, 0, "USD", "msg_abc", FromCache: false));

        await fixture.Job.ExecuteAsync(
            fixture.TenantId, fixture.Operation.Id.Value, QuestionVariantSourceType.Reading, fixture.SourceQuestion.Id.Value, 1,
            CancellationToken.None);

        Assert.Equal(AiOperationStatus.Succeeded, fixture.Operation.Status);
        var draft = Assert.Single(fixture.Drafts.Drafts);
        Assert.Equal(fixture.Operation.Id.Value, draft.Id.Value);
        Assert.Equal(QuestionVariantStatus.Pending, draft.Status);
        Assert.Equal(1, draft.DifficultyDelta);
        Assert.Contains("What primarily drives", draft.DraftContent);
        Assert.Contains("MultipleChoice", draft.DraftContent);
    }

    [Fact]
    public async Task Malformed_structured_output_fails_the_operation_without_creating_a_draft()
    {
        var fixture = CreateFixture();
        fixture.Gateway.NextResult = Result.Success(new IELTSLms.Abstractions.AI.AiGatewayInvocationResult(
            "not json at all", "claude-3-5-haiku", 12, 8, 120, 0, "USD", "msg_abc", FromCache: false));

        await fixture.Job.ExecuteAsync(
            fixture.TenantId, fixture.Operation.Id.Value, QuestionVariantSourceType.Reading, fixture.SourceQuestion.Id.Value, 1,
            CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Empty(fixture.Drafts.Drafts);
    }

    [Fact]
    public async Task Missing_source_question_fails_without_calling_the_gateway()
    {
        var fixture = CreateFixture();

        await fixture.Job.ExecuteAsync(
            fixture.TenantId, fixture.Operation.Id.Value, QuestionVariantSourceType.Reading, Guid.NewGuid(), 1, CancellationToken.None);

        Assert.Equal(AiOperationStatus.Failed, fixture.Operation.Status);
        Assert.Equal("NOT_FOUND_QUESTION_VARIANT_SOURCE", fixture.Operation.ErrorCode);
        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
        Assert.Empty(fixture.Drafts.Drafts);
    }

    [Fact]
    public async Task Already_terminal_operation_is_not_reprocessed()
    {
        var fixture = CreateFixture();
        fixture.Operation.MarkSucceeded(AiResponseId.From(Guid.NewGuid()), Now);

        await fixture.Job.ExecuteAsync(
            fixture.TenantId, fixture.Operation.Id.Value, QuestionVariantSourceType.Reading, fixture.SourceQuestion.Id.Value, 1,
            CancellationToken.None);

        Assert.Equal(0, fixture.Gateway.InvokeCallCount);
    }
}
