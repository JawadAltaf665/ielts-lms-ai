using IELTSLms.Application.AI.Prompts;
using IELTSLms.Application.AI.Prompts.ActivatePromptVersion;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI.Prompts;

public sealed class ActivatePromptVersionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private static ActivatePromptVersionCommandHandler CreateHandler(
        FakeAiPromptRepository prompts, FakeUnitOfWork unitOfWork, IEnumerable<IPromptActivationValidator>? activationValidators = null) =>
        new(prompts, unitOfWork, new FakeClock(Now.AddDays(1)), activationValidators ?? []);

    private sealed class FakeActivationValidator(string feature, Result result) : IPromptActivationValidator
    {
        public int CallCount { get; private set; }

        public string Feature => feature;

        public Task<Result> ValidateAsync(Guid tenantId, AiPrompt candidate, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(result);
        }
    }

    private AiPrompt Seed(FakeAiPromptRepository prompts, string scope, int version, bool active = false)
    {
        var prompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", version, scope, "template", Now).Value;
        if (active)
        {
            prompt.Activate(Now);
        }

        prompts.Seed(prompt);
        return prompt;
    }

    [Fact]
    public async Task Activating_a_draft_deactivates_whatever_was_active_in_the_same_slot()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = Seed(prompts, "default", 1, active: true);
        var v2 = Seed(prompts, "default", 2);
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        var result = await handler.Handle(new ActivatePromptVersionCommand(_tenantId, v2.Id.Value, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        Assert.False(v1.IsActive);
        Assert.True(v2.IsActive);
    }

    [Fact]
    public async Task A_different_scope_in_the_same_feature_is_left_untouched()
    {
        var prompts = new FakeAiPromptRepository();
        var controlActive = Seed(prompts, "control", 1, active: true);
        var variantB = Seed(prompts, "variant-b", 1);
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        await handler.Handle(new ActivatePromptVersionCommand(_tenantId, variantB.Id.Value, null), CancellationToken.None);

        Assert.True(controlActive.IsActive);
        Assert.True(variantB.IsActive);
    }

    [Fact]
    public async Task Mismatched_If_Match_is_rejected_with_a_conflict()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = Seed(prompts, "default", 1);
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        var result = await handler.Handle(
            new ActivatePromptVersionCommand(_tenantId, v1.Id.Value, IfMatchVersion: 999), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_AI_PROMPT_VERSION_MISMATCH", result.Error.Code);
        Assert.False(v1.IsActive);
    }

    [Fact]
    public async Task Prompt_belonging_to_another_tenant_is_not_found()
    {
        var prompts = new FakeAiPromptRepository();
        var foreign = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), Guid.NewGuid(), null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(foreign);
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        var result = await handler.Handle(new ActivatePromptVersionCommand(_tenantId, foreign.Id.Value, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_AI_PROMPT", result.Error.Code);
    }

    [Fact]
    public async Task A_feature_specific_activation_validator_that_fails_blocks_activation()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = Seed(prompts, "default", 1);
        var validator = new FakeActivationValidator(
            "grammar-coach", Result.Failure(Error.Validation("BUSINESS_AI_PROMPT_CALIBRATION_FAILED", "nope")));
        var handler = CreateHandler(prompts, new FakeUnitOfWork(), [validator]);

        var result = await handler.Handle(new ActivatePromptVersionCommand(_tenantId, v1.Id.Value, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_AI_PROMPT_CALIBRATION_FAILED", result.Error.Code);
        Assert.False(v1.IsActive);
        Assert.Equal(1, validator.CallCount);
    }

    [Fact]
    public async Task A_validator_for_a_different_feature_is_not_invoked()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = Seed(prompts, "default", 1);
        var validator = new FakeActivationValidator("speaking-examiner", Result.Failure(Error.Validation("X", "x")));
        var handler = CreateHandler(prompts, new FakeUnitOfWork(), [validator]);

        var result = await handler.Handle(new ActivatePromptVersionCommand(_tenantId, v1.Id.Value, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(v1.IsActive);
        Assert.Equal(0, validator.CallCount);
    }

    [Fact]
    public async Task Activating_an_already_active_version_is_a_harmless_no_op()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = Seed(prompts, "default", 1, active: true);
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(prompts, unitOfWork);

        var result = await handler.Handle(new ActivatePromptVersionCommand(_tenantId, v1.Id.Value, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}
