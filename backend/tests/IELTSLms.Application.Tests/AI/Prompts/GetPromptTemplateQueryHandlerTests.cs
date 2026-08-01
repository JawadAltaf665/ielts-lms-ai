using IELTSLms.Application.AI.Prompts.GetPromptTemplate;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Prompts;

public sealed class GetPromptTemplateQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public async Task Returns_the_prompt_when_it_belongs_to_the_caller_tenant()
    {
        var prompts = new FakeAiPromptRepository();
        var prompt = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(prompt);
        var handler = new GetPromptTemplateQueryHandler(prompts);

        var result = await handler.Handle(new GetPromptTemplateQuery(_tenantId, prompt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("template", result.Value.SystemTemplate);
    }

    [Fact]
    public async Task Returns_not_found_for_a_prompt_owned_by_another_tenant()
    {
        var prompts = new FakeAiPromptRepository();
        var foreign = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), Guid.NewGuid(), null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(foreign);
        var handler = new GetPromptTemplateQueryHandler(prompts);

        var result = await handler.Handle(new GetPromptTemplateQuery(_tenantId, foreign.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_AI_PROMPT", result.Error.Code);
    }
}
