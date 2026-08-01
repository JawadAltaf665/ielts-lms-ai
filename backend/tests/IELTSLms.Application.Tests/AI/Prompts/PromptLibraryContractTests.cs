using IELTSLms.Application.AI.Prompts.ActivatePromptVersion;
using IELTSLms.Application.AI.Prompts.CreatePromptVersion;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;

namespace IELTSLms.Application.Tests.AI.Prompts;

/// <summary>
/// Sprint 2's end-to-end proof: authoring and activating a new Grammar Coach version through the admin
/// CQRS surface is picked up by <c>IAiPromptRepository.GetActiveForFeatureAsync</c> — the exact query
/// <c>GrammarCoachingJob</c> (Sprint 1's composition engine) calls — with no change to that job or to
/// <c>RequestGrammarCoachingCommandHandler</c>. Neither is referenced here; this test only proves the
/// shared repository contract updates correctly.
/// </summary>
public sealed class PromptLibraryContractTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Newly_activated_tenant_override_is_immediately_visible_to_the_composition_engines_resolution_query()
    {
        var prompts = new FakeAiPromptRepository();
        var unitOfWork = new FakeUnitOfWork();
        var createHandler = new CreatePromptVersionCommandHandler(prompts, unitOfWork, new FakeClock(Now), new FakeGuidProvider());
        var activateHandler = new ActivatePromptVersionCommandHandler(prompts, unitOfWork, new FakeClock(Now.AddMinutes(1)));

        // Before authoring anything, the job's resolution query finds nothing for this tenant.
        var beforeCreate = await prompts.GetActiveForFeatureAsync(_tenantId, "grammar-coach", CancellationToken.None);
        Assert.Null(beforeCreate);

        var created = await createHandler.Handle(
            new CreatePromptVersionCommand(_tenantId, "grammar-coach", "default", "Updated coaching instructions.", null),
            CancellationToken.None);
        Assert.True(created.IsSuccess);

        // Authoring alone does not roll it out — a draft is invisible to the resolution query.
        var afterCreate = await prompts.GetActiveForFeatureAsync(_tenantId, "grammar-coach", CancellationToken.None);
        Assert.Null(afterCreate);

        var activated = await activateHandler.Handle(
            new ActivatePromptVersionCommand(_tenantId, created.Value.Id, null), CancellationToken.None);
        Assert.True(activated.IsSuccess);

        var afterActivate = await prompts.GetActiveForFeatureAsync(_tenantId, "grammar-coach", CancellationToken.None);
        Assert.NotNull(afterActivate);
        Assert.Equal("Updated coaching instructions.", afterActivate!.SystemTemplate);
        Assert.Equal(1, afterActivate.PromptVersion);
    }
}
