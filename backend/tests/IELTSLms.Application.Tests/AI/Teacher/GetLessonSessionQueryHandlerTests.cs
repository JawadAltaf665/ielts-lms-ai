using IELTSLms.Application.AI.Teacher.Queries.GetLessonSession;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Domain.AI;

namespace IELTSLms.Application.Tests.AI.Teacher;

public sealed class GetLessonSessionQueryHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Unknown_session_is_reported_as_not_found()
    {
        var handler = new GetLessonSessionQueryHandler(new FakeLessonSessionRepository(), new FakeConversationMessageRepository());

        var result = await handler.Handle(
            new GetLessonSessionQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LESSON_SESSION", result.Error.Code);
    }

    [Fact]
    public async Task Cross_tenant_access_is_reported_as_not_found_not_forbidden()
    {
        var lessonSessions = new FakeLessonSessionRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var session = LessonSession.Start(
            LessonSessionId.From(Guid.NewGuid()), tenantId, userId, Guid.NewGuid(), ConversationId.From(Guid.NewGuid()), Now);
        lessonSessions.Seed(session);
        var handler = new GetLessonSessionQueryHandler(lessonSessions, new FakeConversationMessageRepository());

        var result = await handler.Handle(new GetLessonSessionQuery(Guid.NewGuid(), userId, session.Id.Value), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_LESSON_SESSION", result.Error.Code);
    }

    [Fact]
    public async Task Returns_the_session_state_and_its_conversation_history()
    {
        var lessonSessions = new FakeLessonSessionRepository();
        var messages = new FakeConversationMessageRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var conversationId = ConversationId.From(Guid.NewGuid());
        var session = LessonSession.Start(LessonSessionId.From(Guid.NewGuid()), tenantId, userId, Guid.NewGuid(), conversationId, Now);
        lessonSessions.Seed(session);
        messages.Seed(ConversationMessage.Create(
            ConversationMessageId.From(Guid.NewGuid()), tenantId, conversationId, ConversationMessageRole.Assistant,
            "Let's begin.", 1, 5, null, Now).Value);
        var handler = new GetLessonSessionQueryHandler(lessonSessions, messages);

        var result = await handler.Handle(new GetLessonSessionQuery(tenantId, userId, session.Id.Value), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Explain", result.Value.Session.CurrentStep);
        Assert.Single(result.Value.Messages);
        Assert.Equal("Let's begin.", result.Value.Messages[0].Content);
    }
}
