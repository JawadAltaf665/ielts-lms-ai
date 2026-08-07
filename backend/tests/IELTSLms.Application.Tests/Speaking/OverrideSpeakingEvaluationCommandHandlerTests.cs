using IELTSLms.Application.Speaking.Commands.OverrideSpeakingEvaluation;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class OverrideSpeakingEvaluationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static SpeakingEvaluation SeedEvaluation(FakeSpeakingEvaluationRepository evaluations, Guid tenantId)
    {
        var evaluation = SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(Guid.NewGuid()), tenantId, SpeakingAttemptId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m, 0.9m,
            "feedback", 0.7m, 3.0m, null, null, Now).Value;
        evaluations.Seed(evaluation);
        return evaluation;
    }

    [Fact]
    public async Task Non_teacher_is_rejected()
    {
        var evaluations = new FakeSpeakingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var evaluation = SeedEvaluation(evaluations, tenantId);
        var handler = new OverrideSpeakingEvaluationCommandHandler(
            evaluations, new FakeUnitOfWork(), new FakeClock(Now.AddDays(1)), new FakeCurrentUserService { Roles = ["Student"] });

        var result = await handler.Handle(
            new OverrideSpeakingEvaluationCommand(tenantId, Guid.NewGuid(), evaluation.Id.Value, 6.0m, 6.0m, 6.0m, 6.0m, "reason"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_SPEAKING_EVALUATION_OVERRIDE_TEACHER_ONLY", result.Error.Code);
        Assert.False(evaluation.IsOverridden);
    }

    [Fact]
    public async Task Teacher_override_preserves_the_original_AI_scores()
    {
        var evaluations = new FakeSpeakingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var evaluation = SeedEvaluation(evaluations, tenantId);
        var originalFluencyCoherence = evaluation.FluencyCoherence;
        var teacherId = Guid.NewGuid();
        var handler = new OverrideSpeakingEvaluationCommandHandler(
            evaluations, new FakeUnitOfWork(), new FakeClock(Now.AddDays(1)), new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(
            new OverrideSpeakingEvaluationCommand(tenantId, teacherId, evaluation.Id.Value, 6.0m, 6.0m, 6.0m, 6.0m, "Too generous."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsOverridden);
        Assert.Equal(6.0m, result.Value.OverriddenFluencyCoherence);
        Assert.Equal(teacherId, result.Value.OverriddenBy);
        Assert.Equal(originalFluencyCoherence, result.Value.FluencyCoherence);
    }

    [Fact]
    public async Task Unknown_evaluation_is_rejected()
    {
        var evaluations = new FakeSpeakingEvaluationRepository();
        var handler = new OverrideSpeakingEvaluationCommandHandler(
            evaluations, new FakeUnitOfWork(), new FakeClock(Now), new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(
            new OverrideSpeakingEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 6.0m, 6.0m, 6.0m, 6.0m, "reason"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_SPEAKING_EVALUATION", result.Error.Code);
    }
}
