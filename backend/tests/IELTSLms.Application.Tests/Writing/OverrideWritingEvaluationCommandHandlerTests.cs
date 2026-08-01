using IELTSLms.Application.Tests.Writing.Fakes;
using IELTSLms.Application.Writing.Commands.OverrideWritingEvaluation;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.Tests.Writing;

public sealed class OverrideWritingEvaluationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static WritingEvaluation SeedEvaluation(FakeWritingEvaluationRepository evaluations, Guid tenantId)
    {
        var evaluation = WritingEvaluation.Create(
            WritingEvaluationId.From(Guid.NewGuid()), tenantId, WritingSubmissionId.From(Guid.NewGuid()), 7.0m, 6.5m, 7.0m, 6.5m, 0.9m,
            "feedback", 0.7m, null, null, Now).Value;
        evaluations.Seed(evaluation);
        return evaluation;
    }

    [Fact]
    public async Task Non_teacher_is_rejected()
    {
        var evaluations = new FakeWritingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var evaluation = SeedEvaluation(evaluations, tenantId);
        var handler = new OverrideWritingEvaluationCommandHandler(
            evaluations, new FakeUnitOfWork(), new FakeClock(Now.AddDays(1)), new FakeCurrentUserService { Roles = ["Student"] });

        var result = await handler.Handle(
            new OverrideWritingEvaluationCommand(tenantId, Guid.NewGuid(), evaluation.Id.Value, 6.0m, 6.0m, 6.0m, 6.0m, "reason"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_WRITING_EVALUATION_OVERRIDE_TEACHER_ONLY", result.Error.Code);
        Assert.False(evaluation.IsOverridden);
    }

    [Fact]
    public async Task Teacher_override_preserves_the_original_AI_scores()
    {
        var evaluations = new FakeWritingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var evaluation = SeedEvaluation(evaluations, tenantId);
        var originalTaskResponse = evaluation.TaskResponse;
        var teacherId = Guid.NewGuid();
        var handler = new OverrideWritingEvaluationCommandHandler(
            evaluations, new FakeUnitOfWork(), new FakeClock(Now.AddDays(1)), new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(
            new OverrideWritingEvaluationCommand(tenantId, teacherId, evaluation.Id.Value, 6.0m, 6.0m, 6.0m, 6.0m, "Too generous."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsOverridden);
        Assert.Equal(6.0m, result.Value.OverriddenTaskResponse);
        Assert.Equal(teacherId, result.Value.OverriddenBy);
        Assert.Equal(originalTaskResponse, result.Value.TaskResponse);
    }

    [Fact]
    public async Task Unknown_evaluation_is_rejected()
    {
        var evaluations = new FakeWritingEvaluationRepository();
        var handler = new OverrideWritingEvaluationCommandHandler(
            evaluations, new FakeUnitOfWork(), new FakeClock(Now), new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(
            new OverrideWritingEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 6.0m, 6.0m, 6.0m, 6.0m, "reason"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_WRITING_EVALUATION", result.Error.Code);
    }
}
