using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Domain.Tests.Learning;

public sealed class LearningRecommendationTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static LearningRecommendation CreateActive() =>
        LearningRecommendation.Create(
            LearningRecommendationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(),
            """[{"skill":"Writing","title":"Practice Task 2","rationale":"weak","priority":1}]""",
            AiResponseId.From(Guid.NewGuid()), Now).Value;

    [Fact]
    public void Create_starts_active()
    {
        var recommendation = CreateActive();

        Assert.Equal(LearningRecommendationStatus.Active, recommendation.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_without_items_fails(string? itemsJson)
    {
        var result = LearningRecommendation.Create(
            LearningRecommendationId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), itemsJson!, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_LEARNING_RECOMMENDATION_ITEMS_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Supersede_from_active_succeeds()
    {
        var recommendation = CreateActive();

        var result = recommendation.Supersede(Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(LearningRecommendationStatus.Superseded, recommendation.Status);
    }

    [Fact]
    public void Dismiss_from_active_succeeds()
    {
        var recommendation = CreateActive();

        var result = recommendation.Dismiss(Now.AddDays(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(LearningRecommendationStatus.Dismissed, recommendation.Status);
    }

    [Fact]
    public void Dismiss_an_already_superseded_set_fails()
    {
        var recommendation = CreateActive();
        recommendation.Supersede(Now.AddDays(1));

        var result = recommendation.Dismiss(Now.AddDays(2));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LEARNING_RECOMMENDATION_NOT_ACTIVE", result.Error.Code);
    }

    [Fact]
    public void Supersede_an_already_dismissed_set_fails()
    {
        var recommendation = CreateActive();
        recommendation.Dismiss(Now.AddDays(1));

        var result = recommendation.Supersede(Now.AddDays(2));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LEARNING_RECOMMENDATION_NOT_ACTIVE", result.Error.Code);
    }
}
