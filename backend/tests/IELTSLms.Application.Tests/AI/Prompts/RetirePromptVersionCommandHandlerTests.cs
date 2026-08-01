using IELTSLms.Application.AI.Prompts.RetirePromptVersion;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Prompts;

public sealed class RetirePromptVersionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    private static RetirePromptVersionCommandHandler CreateHandler(FakeAiPromptRepository prompts, FakeUnitOfWork unitOfWork) =>
        new(prompts, unitOfWork, new FakeClock(Now.AddDays(1)));

    [Fact]
    public async Task Retiring_an_active_version_is_rejected_until_reassigned()
    {
        var prompts = new FakeAiPromptRepository();
        var prompt = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 1, "default", "template", Now).Value;
        prompt.Activate(Now);
        prompts.Seed(prompt);
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        var result = await handler.Handle(new RetirePromptVersionCommand(_tenantId, prompt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_AI_PROMPT_ACTIVE_CANNOT_RETIRE", result.Error.Code);
        Assert.False(prompt.IsDeleted);
    }

    [Fact]
    public async Task Retiring_an_inactive_version_soft_deletes_it()
    {
        var prompts = new FakeAiPromptRepository();
        var prompt = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(prompt);
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(prompts, unitOfWork);

        var result = await handler.Handle(new RetirePromptVersionCommand(_tenantId, prompt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(prompt.IsDeleted);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Retiring_a_prompt_from_another_tenant_is_not_found()
    {
        var prompts = new FakeAiPromptRepository();
        var foreign = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), Guid.NewGuid(), null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(foreign);
        var handler = CreateHandler(prompts, new FakeUnitOfWork());

        var result = await handler.Handle(new RetirePromptVersionCommand(_tenantId, foreign.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_AI_PROMPT", result.Error.Code);
    }
}
