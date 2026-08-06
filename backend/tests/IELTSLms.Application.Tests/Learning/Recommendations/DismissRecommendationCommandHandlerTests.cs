using IELTSLms.Application.Learning.Recommendations.Commands.DismissRecommendation;
using IELTSLms.Application.Tests.Learning.Fakes;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.Recommendations;

public sealed class DismissRecommendationCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Dismissing_an_active_recommendation_marks_it_dismissed()
    {
        var recommendations = new FakeLearningRecommendationRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recommendation = LearningRecommendation.Create(
            LearningRecommendationId.From(Guid.NewGuid()), tenantId, userId,
            """[{"skill":"Writing","title":"Practice","rationale":"weak","priority":1}]""", null, Now).Value;
        recommendations.Seed(recommendation);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new DismissRecommendationCommandHandler(recommendations, unitOfWork, new FakeClock(Now.AddDays(1)));

        var result = await handler.Handle(
            new DismissRecommendationCommand(tenantId, userId, recommendation.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LearningRecommendationStatus.Dismissed, recommendation.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Dismissing_someone_elses_recommendation_returns_not_found_not_the_recommendation()
    {
        var recommendations = new FakeLearningRecommendationRepository();
        var ownerTenantId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var recommendation = LearningRecommendation.Create(
            LearningRecommendationId.From(Guid.NewGuid()), ownerTenantId, ownerUserId,
            """[{"skill":"Writing","title":"Practice","rationale":"weak","priority":1}]""", null, Now).Value;
        recommendations.Seed(recommendation);

        var handler = new DismissRecommendationCommandHandler(recommendations, new FakeUnitOfWork(), new FakeClock(Now));

        // A different user (even in the same tenant) must not be able to dismiss someone else's set.
        var result = await handler.Handle(
            new DismissRecommendationCommand(ownerTenantId, Guid.NewGuid(), recommendation.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LEARNING_RECOMMENDATION", result.Error.Code);
        Assert.Equal(LearningRecommendationStatus.Active, recommendation.Status);
    }

    [Fact]
    public async Task Dismissing_an_already_superseded_recommendation_fails()
    {
        var recommendations = new FakeLearningRecommendationRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recommendation = LearningRecommendation.Create(
            LearningRecommendationId.From(Guid.NewGuid()), tenantId, userId,
            """[{"skill":"Writing","title":"Practice","rationale":"weak","priority":1}]""", null, Now).Value;
        recommendation.Supersede(Now.AddHours(1));
        recommendations.Seed(recommendation);

        var handler = new DismissRecommendationCommandHandler(recommendations, new FakeUnitOfWork(), new FakeClock(Now.AddDays(1)));

        var result = await handler.Handle(
            new DismissRecommendationCommand(tenantId, userId, recommendation.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LEARNING_RECOMMENDATION_NOT_ACTIVE", result.Error.Code);
    }
}
