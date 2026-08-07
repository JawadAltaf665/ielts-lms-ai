using IELTSLms.Application.AI.Prompts.RollbackPromptVersion;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Prompts;

public sealed class RollbackPromptVersionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _actorId = Guid.NewGuid();

    private static RollbackPromptVersionCommandHandler CreateHandler(
        FakeAiPromptRepository prompts, FakeUnitOfWork unitOfWork, FakeAuditLogWriter auditLogWriter, DateTimeOffset now) =>
        new(prompts, unitOfWork, new FakeClock(now), auditLogWriter, new FakeCorrelationIdProvider());

    [Fact]
    public async Task Activate_then_rollback_round_trip_restores_the_exact_prior_version()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 1, "default", "template v1", Now).Value;
        v1.Activate(Now);
        prompts.Seed(v1);
        var v2 = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 2, "default", "template v2", Now).Value;
        prompts.Seed(v2);

        // Activation swap (mirrors ActivatePromptVersionCommandHandler): v2 goes live, v1 steps down.
        v1.Deactivate(Now.AddHours(1));
        v2.Activate(Now.AddHours(1));

        var auditLogWriter = new FakeAuditLogWriter();
        var handler = CreateHandler(prompts, new FakeUnitOfWork(), auditLogWriter, Now.AddHours(2));

        var result = await handler.Handle(
            new RollbackPromptVersionCommand(_tenantId, v2.Id.Value, _actorId, "Regression in v2 tone."), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Version);
        Assert.True(v1.IsActive);
        Assert.False(v2.IsActive);

        var entry = Assert.Single(auditLogWriter.Entries);
        Assert.Equal("Rollback", entry.Action);
        Assert.Equal(_actorId, entry.UserId);
        Assert.Equal(_tenantId, entry.TenantId);
        Assert.Contains("Regression in v2 tone.", entry.Changes);
    }

    [Fact]
    public async Task Rolling_back_a_version_that_is_not_currently_active_is_rejected()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 1, "default", "template", Now).Value;
        prompts.Seed(v1);
        var handler = CreateHandler(prompts, new FakeUnitOfWork(), new FakeAuditLogWriter(), Now);

        var result = await handler.Handle(
            new RollbackPromptVersionCommand(_tenantId, v1.Id.Value, _actorId, "reason"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_AI_PROMPT_NOT_ACTIVE", result.Error.Code);
    }

    [Fact]
    public async Task Rolling_back_the_first_ever_version_with_nothing_before_it_is_rejected()
    {
        var prompts = new FakeAiPromptRepository();
        var v1 = AiPrompt.Create(AiPromptId.From(Guid.NewGuid()), _tenantId, null, "grammar-coach", 1, "default", "template", Now).Value;
        v1.Activate(Now);
        prompts.Seed(v1);
        var handler = CreateHandler(prompts, new FakeUnitOfWork(), new FakeAuditLogWriter(), Now);

        var result = await handler.Handle(
            new RollbackPromptVersionCommand(_tenantId, v1.Id.Value, _actorId, "reason"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_AI_PROMPT_NO_PRIOR_VERSION", result.Error.Code);
    }
}
