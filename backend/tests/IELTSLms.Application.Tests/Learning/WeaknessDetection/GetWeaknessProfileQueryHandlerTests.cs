using IELTSLms.Application.Learning.WeaknessDetection.Queries.GetWeaknessProfile;
using IELTSLms.Application.Tests.Learning.Fakes;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.WeaknessDetection;

public sealed class GetWeaknessProfileQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task No_profile_and_no_activity_returns_insufficient_data()
    {
        var handler = new GetWeaknessProfileQueryHandler(new FakeWeaknessProfileRepository(), new FakeProgressTrackingRepository());

        var result = await handler.Handle(new GetWeaknessProfileQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("InsufficientData", result.Value.Status);
        Assert.Null(result.Value.WeaknessProfileId);
    }

    [Fact]
    public async Task No_profile_but_sufficient_activity_returns_not_computed_yet()
    {
        var progressTracking = new FakeProgressTrackingRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var grammar = ProgressTracking.Create(ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, LearningSkill.Grammar, Now);
        grammar.RecordActivity(Now);
        grammar.RecordActivity(Now);
        grammar.RecordActivity(Now);
        progressTracking.Seed(grammar);
        var reading = ProgressTracking.Create(ProgressTrackingId.From(Guid.NewGuid()), tenantId, userId, LearningSkill.Reading, Now);
        reading.RecordActivity(Now);
        reading.RecordActivity(Now);
        progressTracking.Seed(reading);

        var handler = new GetWeaknessProfileQueryHandler(new FakeWeaknessProfileRepository(), progressTracking);

        var result = await handler.Handle(new GetWeaknessProfileQuery(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("NotComputedYet", result.Value.Status);
    }

    [Fact]
    public async Task An_existing_profile_returns_ready_with_its_scores()
    {
        var weaknessProfiles = new FakeWeaknessProfileRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var profile = WeaknessProfile.Create(
            WeaknessProfileId.From(Guid.NewGuid()), tenantId, userId,
            """[{"skill":"Writing","score":85,"sampleSize":4}]""", AiResponseId.From(Guid.NewGuid()), Now).Value;
        weaknessProfiles.Seed(profile);

        var handler = new GetWeaknessProfileQueryHandler(weaknessProfiles, new FakeProgressTrackingRepository());

        var result = await handler.Handle(new GetWeaknessProfileQuery(tenantId, userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ready", result.Value.Status);
        var score = Assert.Single(result.Value.Scores!);
        Assert.Equal(LearningSkill.Writing, score.Skill);
        Assert.Equal(85, score.Score);
    }
}
