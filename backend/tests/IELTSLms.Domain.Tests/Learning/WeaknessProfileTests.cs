using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Domain.Tests.Learning;

public sealed class WeaknessProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_with_scores_succeeds()
    {
        var result = WeaknessProfile.Create(
            WeaknessProfileId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(),
            """[{"skill":"Writing","score":80,"sampleSize":3}]""", AiResponseId.From(Guid.NewGuid()), Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, result.Value.ComputedAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_without_scores_fails(string? scoresJson)
    {
        var result = WeaknessProfile.Create(
            WeaknessProfileId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(),
            scoresJson!, aiResponseId: null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WEAKNESS_PROFILE_SCORES_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Recompute_overwrites_scores_and_computed_at_in_place()
    {
        var profile = WeaknessProfile.Create(
            WeaknessProfileId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(),
            """[{"skill":"Writing","score":80,"sampleSize":3}]""", AiResponseId.From(Guid.NewGuid()), Now).Value;

        var recompute = profile.Recompute(
            """[{"skill":"Writing","score":40,"sampleSize":5}]""", AiResponseId.From(Guid.NewGuid()), Now.AddDays(1));

        Assert.True(recompute.IsSuccess);
        Assert.Contains("40", profile.SkillScoresJson);
        Assert.Equal(Now.AddDays(1), profile.ComputedAtUtc);
    }

    [Fact]
    public void Recompute_without_scores_fails_and_leaves_the_profile_unchanged()
    {
        var profile = WeaknessProfile.Create(
            WeaknessProfileId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(),
            """[{"skill":"Writing","score":80,"sampleSize":3}]""", AiResponseId.From(Guid.NewGuid()), Now).Value;

        var recompute = profile.Recompute(string.Empty, null, Now.AddDays(1));

        Assert.True(recompute.IsFailure);
        Assert.Equal(Now, profile.ComputedAtUtc);
    }
}
