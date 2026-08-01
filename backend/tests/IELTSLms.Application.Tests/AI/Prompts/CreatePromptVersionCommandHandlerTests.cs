using IELTSLms.Application.AI.Prompts.CreatePromptVersion;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;

namespace IELTSLms.Application.Tests.AI.Prompts;

public sealed class CreatePromptVersionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private static CreatePromptVersionCommandHandler CreateHandler(FakeAiPromptRepository prompts, FakeUnitOfWork unitOfWork) =>
        new(prompts, unitOfWork, new FakeClock(Now), new FakeGuidProvider());

    [Fact]
    public async Task First_version_for_a_new_slot_is_version_1_and_a_draft()
    {
        var prompts = new FakeAiPromptRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(prompts, unitOfWork);

        var result = await handler.Handle(
            new CreatePromptVersionCommand(_tenantId, "grammar-coach", "default", "system template", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Version);
        Assert.False(result.Value.IsActive);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Authoring_again_never_mutates_the_existing_version_it_always_creates_the_next_one()
    {
        var prompts = new FakeAiPromptRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(prompts, unitOfWork);

        var v1 = await handler.Handle(
            new CreatePromptVersionCommand(_tenantId, "grammar-coach", "default", "template v1", null), CancellationToken.None);
        var v2 = await handler.Handle(
            new CreatePromptVersionCommand(_tenantId, "grammar-coach", "default", "template v2", null), CancellationToken.None);

        Assert.Equal(1, v1.Value.Version);
        Assert.Equal(2, v2.Value.Version);
        Assert.NotEqual(v1.Value.Id, v2.Value.Id);

        var versions = await prompts.GetVersionsAsync(_tenantId, "grammar-coach", "default", CancellationToken.None);
        Assert.Equal(2, versions.Count);
        Assert.Contains(versions, p => p.PromptVersion == 1 && p.SystemTemplate == "template v1");
        Assert.Contains(versions, p => p.PromptVersion == 2 && p.SystemTemplate == "template v2");
    }

    [Fact]
    public async Task A_different_scope_gets_its_own_version_sequence()
    {
        var prompts = new FakeAiPromptRepository();
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        await handler.Handle(
            new CreatePromptVersionCommand(_tenantId, "grammar-coach", "control", "template", null), CancellationToken.None);
        var variantB = await handler.Handle(
            new CreatePromptVersionCommand(_tenantId, "grammar-coach", "variant-b", "template", null), CancellationToken.None);

        Assert.Equal(1, variantB.Value.Version);
    }

    [Fact]
    public async Task Blank_feature_fails_domain_validation()
    {
        var handler = CreateHandler(new FakeAiPromptRepository(), new FakeUnitOfWork());

        var result = await handler.Handle(
            new CreatePromptVersionCommand(_tenantId, "  ", "default", "template", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_AI_PROMPT_FEATURE_REQUIRED", result.Error.Code);
    }
}
