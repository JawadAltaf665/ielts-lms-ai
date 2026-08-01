using IELTSLms.Application.Speaking.Queries.GetSpeakingAttempt;
using IELTSLms.Application.Tests.Speaking.Fakes;
using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;

namespace IELTSLms.Application.Tests.Speaking;

public sealed class GetSpeakingAttemptQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static SpeakingAttempt SeedAttempt(FakeSpeakingAttemptRepository attempts, Guid tenantId, Guid userId)
    {
        var attempt = SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), tenantId, userId, SpeakingTaskId.From(Guid.NewGuid()),
            FileAssetId.From(Guid.NewGuid()), 45, Now).Value;
        attempts.Seed(attempt);
        return attempt;
    }

    [Fact]
    public async Task Owner_can_view_their_own_attempt()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var attempt = SeedAttempt(attempts, tenantId, userId);
        var handler = new GetSpeakingAttemptQueryHandler(attempts, new FakeCurrentUserService());

        var result = await handler.Handle(new GetSpeakingAttemptQuery(tenantId, userId, attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(attempt.Id.Value, result.Value.Id);
    }

    [Fact]
    public async Task Another_users_attempt_is_not_found_rather_than_forbidden()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var tenantId = Guid.NewGuid();
        var attempt = SeedAttempt(attempts, tenantId, Guid.NewGuid());
        var handler = new GetSpeakingAttemptQueryHandler(attempts, new FakeCurrentUserService());

        var result = await handler.Handle(new GetSpeakingAttemptQuery(tenantId, Guid.NewGuid(), attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_SPEAKING_ATTEMPT", result.Error.Code);
    }

    [Fact]
    public async Task A_teacher_can_view_any_attempt_in_the_tenant()
    {
        var attempts = new FakeSpeakingAttemptRepository();
        var tenantId = Guid.NewGuid();
        var attempt = SeedAttempt(attempts, tenantId, Guid.NewGuid());
        var handler = new GetSpeakingAttemptQueryHandler(attempts, new FakeCurrentUserService { Roles = ["Teacher"] });

        var result = await handler.Handle(new GetSpeakingAttemptQuery(tenantId, Guid.NewGuid(), attempt.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
