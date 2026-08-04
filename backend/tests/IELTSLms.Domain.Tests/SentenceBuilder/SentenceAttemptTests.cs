using IELTSLms.Domain.AI;
using IELTSLms.Domain.SentenceBuilder;
using IELTSLms.Domain.SentenceBuilder.Events;

namespace IELTSLms.Domain.Tests.SentenceBuilder;

public sealed class SentenceAttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Submit_raises_SentenceAttemptSubmitted_with_the_scored_result()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exerciseId = SentenceExerciseId.From(Guid.NewGuid());

        var attempt = SentenceAttempt.Submit(
            SentenceAttemptId.From(Guid.NewGuid()), tenantId, userId, exerciseId, "Although it rained, she would go.", 100, Now);

        Assert.Equal(100, attempt.Score);
        Assert.Null(attempt.AiResponseId);
        Assert.Null(attempt.Feedback);

        var raised = Assert.Single(attempt.DomainEvents);
        var submitted = Assert.IsType<SentenceAttemptSubmittedDomainEvent>(raised);
        Assert.Equal(tenantId, submitted.TenantId);
        Assert.Equal(userId, submitted.UserId);
        Assert.Equal(exerciseId.Value, submitted.SentenceExerciseId);
        Assert.Equal(attempt.Id.Value, submitted.SentenceAttemptId);
    }

    [Fact]
    public void AttachAiFeedback_populates_the_response_id_and_feedback()
    {
        var attempt = SentenceAttempt.Submit(
            SentenceAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), SentenceExerciseId.From(Guid.NewGuid()),
            "text", 50, Now);
        var responseId = AiResponseId.From(Guid.NewGuid());

        attempt.AttachAiFeedback(responseId, "Good attempt.", Now);

        Assert.Equal(responseId, attempt.AiResponseId);
        Assert.Equal("Good attempt.", attempt.Feedback);
    }

    [Fact]
    public void AttachAiFeedback_is_a_no_op_once_already_attached()
    {
        var attempt = SentenceAttempt.Submit(
            SentenceAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), SentenceExerciseId.From(Guid.NewGuid()),
            "text", 50, Now);
        var firstResponseId = AiResponseId.From(Guid.NewGuid());
        attempt.AttachAiFeedback(firstResponseId, "First feedback.", Now);

        attempt.AttachAiFeedback(AiResponseId.From(Guid.NewGuid()), "Second feedback.", Now);

        Assert.Equal(firstResponseId, attempt.AiResponseId);
        Assert.Equal("First feedback.", attempt.Feedback);
    }
}
