using IELTSLms.Domain.Grammar;
using IELTSLms.Domain.Grammar.Events;

namespace IELTSLms.Domain.Tests.Grammar;

public sealed class GrammarAttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Submit_raises_GrammarCompleted_with_the_scored_result()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var topicId = Guid.NewGuid();
        var exerciseId = GrammarExerciseId.From(Guid.NewGuid());

        var attempt = GrammarAttempt.Submit(
            GrammarAttemptId.From(Guid.NewGuid()), tenantId, userId, exerciseId, topicId, "[]", rawScore: 3, maxScore: 5, Now);

        Assert.Equal(3, attempt.RawScore);
        Assert.Equal(5, attempt.MaxScore);

        var raised = Assert.Single(attempt.DomainEvents);
        var completed = Assert.IsType<GrammarCompletedDomainEvent>(raised);
        Assert.Equal(tenantId, completed.TenantId);
        Assert.Equal(userId, completed.UserId);
        Assert.Equal(topicId, completed.GrammarTopicId);
        Assert.Equal(exerciseId.Value, completed.GrammarExerciseId);
        Assert.Equal(attempt.Id.Value, completed.GrammarAttemptId);
        Assert.Equal(3, completed.RawScore);
        Assert.Equal(5, completed.MaxScore);
    }
}
