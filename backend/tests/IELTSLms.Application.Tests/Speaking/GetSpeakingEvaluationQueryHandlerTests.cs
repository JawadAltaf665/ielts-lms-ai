using IELTSLms.Application.Speaking.Queries.GetSpeakingEvaluation;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class GetSpeakingEvaluationQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (SpeakingAttempt Attempt, SpeakingEvaluation Evaluation) SeedEvaluatedAttempt(
        FakeSpeakingAttemptRepository attempts, FakeSpeakingEvaluationRepository evaluations, Guid tenantId, Guid userId)
    {
        var attempt = SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), tenantId, userId, SpeakingTaskId.From(Guid.NewGuid()),
            FileAssetId.From(Guid.NewGuid()), 45, Now).Value;
        attempts.Seed(attempt);

        var evaluation = SpeakingEvaluation.Create(
            SpeakingEvaluationId.From(Guid.NewGuid()), tenantId, attempt.Id, 7.0m, 6.5m, 7.0m, 6.5m, 0.9m, "feedback", 0.7m, null, null, Now)
            .Value;
        evaluations.Seed(evaluation);

        return (attempt, evaluation);
    }

    [Fact]
    public async Task Owner_can_view_their_own_evaluation()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var evaluations = new FakeSpeakingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var (_, evaluation) = SeedEvaluatedAttempt(attempts, evaluations, tenantId, userId);
        var handler = new GetSpeakingEvaluationQueryHandler(evaluations, attempts, new FakeCurrentUserService());

        var result = await handler.Handle(new GetSpeakingEvaluationQuery(tenantId, userId, evaluation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(evaluation.Id.Value, result.Value.Id);
    }

    [Fact]
    public async Task Another_users_evaluation_is_not_found_rather_than_forbidden()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var evaluations = new FakeSpeakingEvaluationRepository();
        var tenantId = Guid.NewGuid();
        var (_, evaluation) = SeedEvaluatedAttempt(attempts, evaluations, tenantId, Guid.NewGuid());
        var handler = new GetSpeakingEvaluationQueryHandler(evaluations, attempts, new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetSpeakingEvaluationQuery(tenantId, Guid.NewGuid(), evaluation.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_SPEAKING_EVALUATION", result.Error.Code);
    }

    [Fact]
    public async Task Unknown_evaluation_is_rejected()
    {
        var evaluations = new FakeSpeakingEvaluationRepository();
        var handler = new GetSpeakingEvaluationQueryHandler(evaluations, new FakeSpeakingAttemptRepository(), new FakeCurrentUserService());

        var result = await handler.Handle(
            new GetSpeakingEvaluationQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_SPEAKING_EVALUATION", result.Error.Code);
    }
}
