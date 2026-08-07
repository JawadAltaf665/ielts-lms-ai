using IELTSLms.Application.AI.Prompts.ListPromptTemplates;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Prompts;

public sealed class ListPromptTemplatesQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private AiPrompt Seed(FakeAiPromptRepository prompts, string feature, string scope, int version, bool active)
    {
        var prompt = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, feature, version, scope, "template", Now).Value;
        if (active)
        {
            prompt.Activate(Now);
        }

        prompts.Seed(prompt);
        return prompt;
    }

    [Fact]
    public async Task Filters_by_feature_scope_and_active_status()
    {
        var prompts = new FakeAiPromptRepository();
        Seed(prompts, "grammar-coach", "default", 1, active: false);
        Seed(prompts, "grammar-coach", "default", 2, active: true);
        Seed(prompts, "writing-examiner", "default", 1, active: true);
        var foreignTenantPrompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), Guid.NewGuid(), null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(foreignTenantPrompt);

        var handler = new ListPromptTemplatesQueryHandler(prompts);
        var result = await handler.Handle(
            new ListPromptTemplatesQuery(_tenantId, "grammar-coach", "default", true, Page: 1, PageSize: 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(2, item.Version);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task Never_returns_another_tenants_prompts()
    {
        var prompts = new FakeAiPromptRepository();
        var foreignTenantPrompt = AiPrompt.Create(
            AiPromptId.From(Guid.NewGuid()), Guid.NewGuid(), null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(foreignTenantPrompt);

        var handler = new ListPromptTemplatesQueryHandler(prompts);
        var result = await handler.Handle(
            new ListPromptTemplatesQuery(_tenantId, null, null, null, Page: 1, PageSize: 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }
}
