using IELTSLms.Application.SentenceBuilder.Commands.CreateSentenceExercise;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Application.Tests.SentenceBuilder.Fakes;
using IELTSLms.Domain.SentenceBuilder;

namespace IELTSLms.Application.Tests.SentenceBuilder;

public sealed class CreateSentenceExerciseCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Creates_a_tenant_owned_exercise()
    {
        var exercises = new FakeSentenceExerciseRepository();
        var handler = new CreateSentenceExerciseCommandHandler(exercises, new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());
        var tenantId = Guid.NewGuid();

        var result = await handler.Handle(
            new CreateSentenceExerciseCommand(tenantId, "Write a concessive sentence.", ["although", "would"], SentenceDifficulty.Intermediate),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsGlobal);
        Assert.Equal(tenantId, result.Value.TenantId);
        Assert.Equal(["although", "would"], result.Value.TargetPattern);
    }

    [Fact]
    public async Task Rejects_a_blank_prompt()
    {
        var handler = new CreateSentenceExerciseCommandHandler(
            new FakeSentenceExerciseRepository(), new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider());

        var result = await handler.Handle(
            new CreateSentenceExerciseCommand(Guid.NewGuid(), "", ["although"], SentenceDifficulty.Beginner), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SENTENCE_EXERCISE_PROMPT_REQUIRED", result.Error.Code);
    }
}
