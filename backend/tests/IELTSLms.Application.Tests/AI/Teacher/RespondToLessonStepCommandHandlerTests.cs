using IELTSLms.Application.AI.Teacher.Commands.RespondToLessonStep;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Teacher;

public sealed class RespondToLessonStepCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed record Fixture(
        FakeLessonSessionRepository LessonSessions, FakeAiOperationRepository Operations, FakeUnitOfWork UnitOfWork,
        FakeEntitlementService Entitlements, FakeQuotaService Quotas, FakeBackgroundJobService BackgroundJobs,
        RespondToLessonStepCommandHandler Handler, Guid TenantId, Guid UserId, LessonSession Session);

    private static Fixture CreateFixture()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lessonSessions = new FakeLessonSessionRepository();
        var operations = new FakeAiOperationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var entitlements = new FakeEntitlementService();
        var quotas = new FakeQuotaService();
        var backgroundJobs = new FakeBackgroundJobService();

        var session = LessonSession.Start(
            LessonSessionId.From(Guid.NewGuid()), tenantId, userId, Guid.NewGuid(), ConversationId.From(Guid.NewGuid()), Now);
        lessonSessions.Seed(session);

        var handler = new RespondToLessonStepCommandHandler(
            lessonSessions, operations, unitOfWork, entitlements, quotas, backgroundJobs, new FakeClock(Now), new FakeGuidProvider());

        return new Fixture(lessonSessions, operations, unitOfWork, entitlements, quotas, backgroundJobs, handler, tenantId, userId, session);
    }

    [Fact]
    public async Task Not_entitled_is_rejected()
    {
        var fixture = CreateFixture();
        fixture.Entitlements.HasFeature = false;

        var result = await fixture.Handler.Handle(
            new RespondToLessonStepCommand(fixture.TenantId, fixture.UserId, fixture.Session.Id.Value, "OK"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
    }

    [Fact]
    public async Task Unknown_session_is_reported_as_not_found()
    {
        var fixture = CreateFixture();

        var result = await fixture.Handler.Handle(
            new RespondToLessonStepCommand(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "OK"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LESSON_SESSION", result.Error.Code);
    }

    [Fact]
    public async Task Session_owned_by_a_different_user_is_reported_as_not_found_not_forbidden()
    {
        var fixture = CreateFixture();

        var result = await fixture.Handler.Handle(
            new RespondToLessonStepCommand(fixture.TenantId, Guid.NewGuid(), fixture.Session.Id.Value, "OK"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LESSON_SESSION", result.Error.Code);
    }

    [Fact]
    public async Task A_completed_session_rejects_further_responses()
    {
        var fixture = CreateFixture();
        fixture.Session.Advance(Now);
        fixture.Session.RecordCheckAttempt(passed: true, Now);
        fixture.Session.Advance(Now);
        fixture.Session.Advance(Now);
        Assert.Equal(LessonStep.Completed, fixture.Session.CurrentStep);

        var result = await fixture.Handler.Handle(
            new RespondToLessonStepCommand(fixture.TenantId, fixture.UserId, fixture.Session.Id.Value, "OK"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_LESSON_SESSION_COMPLETED", result.Error.Code);
        Assert.Equal(0, fixture.BackgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Quota_exceeded_short_circuits_before_any_job_is_enqueued()
    {
        var fixture = CreateFixture();
        fixture.Quotas.Allowed = false;

        var result = await fixture.Handler.Handle(
            new RespondToLessonStepCommand(fixture.TenantId, fixture.UserId, fixture.Session.Id.Value, "OK"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Equal(0, fixture.BackgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Entitled_and_within_quota_enqueues_the_job_and_returns_queued()
    {
        var fixture = CreateFixture();

        var result = await fixture.Handler.Handle(
            new RespondToLessonStepCommand(fixture.TenantId, fixture.UserId, fixture.Session.Id.Value, "I understand now."), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Equal(1, fixture.BackgroundJobs.EnqueueCallCount);
        var operation = Assert.Single(fixture.Operations.Operations);
        Assert.Equal("I understand now.", operation.InputText);
    }
}
