using IELTSLms.Application.AI.Teacher.Commands.StartLessonSession;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Grammar.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Teacher;

public sealed class StartLessonSessionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private sealed record Fixture(
        FakeLessonSessionRepository LessonSessions, FakeConversationRepository Conversations, FakeAiOperationRepository Operations,
        FakeUnitOfWork UnitOfWork, FakeEntitlementService Entitlements, FakeQuotaService Quotas, FakeBackgroundJobService BackgroundJobs,
        StartLessonSessionCommandHandler Handler, Guid TenantId, Guid UserId);

    private static Fixture CreateFixture()
    {
        var lessonSessions = new FakeLessonSessionRepository();
        var conversations = new FakeConversationRepository();
        var operations = new FakeAiOperationRepository();
        var unitOfWork = new FakeUnitOfWork();
        var entitlements = new FakeEntitlementService();
        var quotas = new FakeQuotaService();
        var backgroundJobs = new FakeBackgroundJobService();

        var handler = new StartLessonSessionCommandHandler(
            lessonSessions, conversations, operations, unitOfWork, entitlements, quotas, backgroundJobs, new FakeClock(Now),
            new FakeGuidProvider());

        return new Fixture(
            lessonSessions, conversations, operations, unitOfWork, entitlements, quotas, backgroundJobs, handler, Guid.NewGuid(), Guid.NewGuid());
    }

    [Fact]
    public async Task Not_entitled_is_rejected()
    {
        var fixture = CreateFixture();
        fixture.Entitlements.HasFeature = false;

        var result = await fixture.Handler.Handle(
            new StartLessonSessionCommand(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "Conditionals"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTHZ_AI_FEATURE_NOT_ENTITLED", result.Error.Code);
        Assert.Equal(0, fixture.BackgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task An_existing_active_session_for_the_same_lesson_is_rejected_without_touching_quota()
    {
        var fixture = CreateFixture();
        var lessonId = Guid.NewGuid();
        var existing = LessonSession.Start(
            LessonSessionId.From(Guid.NewGuid()), fixture.TenantId, fixture.UserId, lessonId, ConversationId.From(Guid.NewGuid()), Now);
        fixture.LessonSessions.Seed(existing);

        var result = await fixture.Handler.Handle(
            new StartLessonSessionCommand(fixture.TenantId, fixture.UserId, lessonId, "Conditionals"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LESSON_SESSION_ACTIVE_EXISTS", result.Error.Code);
        Assert.Equal(0, fixture.Quotas.ConsumeCallCount);
    }

    [Fact]
    public async Task Quota_exceeded_short_circuits_before_any_job_is_enqueued()
    {
        var fixture = CreateFixture();
        fixture.Quotas.Allowed = false;

        var result = await fixture.Handler.Handle(
            new StartLessonSessionCommand(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "Conditionals"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("RATE_LIMITED_AI_QUOTA_EXCEEDED", result.Error.Code);
        Assert.Equal(0, fixture.BackgroundJobs.EnqueueCallCount);
    }

    [Fact]
    public async Task Entitled_and_within_quota_creates_the_session_and_conversation_and_enqueues_the_explain_step()
    {
        var fixture = CreateFixture();
        var lessonId = Guid.NewGuid();

        var result = await fixture.Handler.Handle(
            new StartLessonSessionCommand(fixture.TenantId, fixture.UserId, lessonId, "Conditionals"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("queued", result.Value.Status);
        Assert.Equal(1, fixture.BackgroundJobs.EnqueueCallCount);

        var session = await fixture.LessonSessions.GetByIdAsync(LessonSessionId.From(result.Value.LessonSessionId));
        Assert.NotNull(session);
        Assert.Equal(LessonStep.Explain, session!.CurrentStep);
        Assert.Equal(lessonId, session.LessonId);

        var conversation = await fixture.Conversations.GetByIdAsync(session.ConversationId);
        Assert.NotNull(conversation);
        Assert.Equal("ai-teacher", conversation!.Feature);

        var operation = Assert.Single(fixture.Operations.Operations);
        Assert.Equal("Conditionals", operation.InputText);
    }
}
