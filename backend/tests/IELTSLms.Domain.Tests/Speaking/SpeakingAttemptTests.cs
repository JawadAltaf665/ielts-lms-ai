using IELTSLms.Domain.Files;
using IELTSLms.Domain.Speaking;
using IELTSLms.Domain.Speaking.Events;

namespace IELTSLms.Domain.Tests.Speaking;

public sealed class SpeakingAttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static SpeakingAttempt CreateRecorded(out Guid tenantId, out Guid userId, out SpeakingTaskId taskId)
    {
        tenantId = Guid.NewGuid();
        userId = Guid.NewGuid();
        taskId = SpeakingTaskId.From(Guid.NewGuid());
        return SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), tenantId, userId, taskId, FileAssetId.From(Guid.NewGuid()), 45, Now).Value;
    }

    [Fact]
    public void Create_starts_in_recorded_with_no_domain_events()
    {
        var attempt = CreateRecorded(out _, out _, out _);

        Assert.Equal(SpeakingAttemptStatus.Recorded, attempt.Status);
        Assert.Empty(attempt.DomainEvents);
    }

    [Fact]
    public void Create_with_zero_duration_fails()
    {
        var result = SpeakingAttempt.Create(
            SpeakingAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), SpeakingTaskId.From(Guid.NewGuid()),
            FileAssetId.From(Guid.NewGuid()), 0, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_SPEAKING_DURATION_INVALID", result.Error.Code);
    }

    [Fact]
    public void Submit_transitions_to_submitted_and_raises_SpeakingSubmitted()
    {
        var attempt = CreateRecorded(out var tenantId, out var userId, out var taskId);

        var result = attempt.Submit(Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(SpeakingAttemptStatus.Submitted, attempt.Status);
        Assert.Equal(Now.AddMinutes(1), attempt.SubmittedAtUtc);

        var raised = Assert.Single(attempt.DomainEvents);
        var submitted = Assert.IsType<SpeakingSubmittedDomainEvent>(raised);
        Assert.Equal(tenantId, submitted.TenantId);
        Assert.Equal(userId, submitted.UserId);
        Assert.Equal(taskId.Value, submitted.SpeakingTaskId);
        Assert.Equal(attempt.Id.Value, submitted.SpeakingAttemptId);
    }

    [Fact]
    public void Submitting_an_already_submitted_attempt_fails_rather_than_resubmitting()
    {
        var attempt = CreateRecorded(out _, out _, out _);
        attempt.Submit(Now.AddMinutes(1));

        var second = attempt.Submit(Now.AddMinutes(2));

        Assert.True(second.IsFailure);
        Assert.Equal("CONFLICT_SPEAKING_ATTEMPT_ALREADY_SUBMITTED", second.Error.Code);
    }

    [Fact]
    public void SetTranscript_on_a_submitted_attempt_succeeds()
    {
        var attempt = CreateRecorded(out _, out _, out _);
        attempt.Submit(Now.AddMinutes(1));

        var result = attempt.SetTranscript("This is what the learner said.", Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal("This is what the learner said.", attempt.TranscriptText);
    }

    [Fact]
    public void SetTranscript_on_a_recorded_attempt_fails()
    {
        var attempt = CreateRecorded(out _, out _, out _);

        var result = attempt.SetTranscript("text", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SPEAKING_ATTEMPT_NOT_PENDING_EVALUATION", result.Error.Code);
        Assert.Null(attempt.TranscriptText);
    }

    [Fact]
    public void AttachEvaluation_on_a_submitted_attempt_transitions_to_evaluated_and_raises_SpeakingEvaluated()
    {
        var attempt = CreateRecorded(out var tenantId, out var userId, out var taskId);
        attempt.Submit(Now.AddMinutes(1));
        var evaluationId = SpeakingEvaluationId.From(Guid.NewGuid());

        var result = attempt.AttachEvaluation(evaluationId, 7.0m, Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(SpeakingAttemptStatus.Evaluated, attempt.Status);
        Assert.Equal(evaluationId, attempt.SpeakingEvaluationId);

        var raised = Assert.Single(attempt.DomainEvents, e => e is SpeakingEvaluatedDomainEvent);
        var evaluated = Assert.IsType<SpeakingEvaluatedDomainEvent>(raised);
        Assert.Equal(tenantId, evaluated.TenantId);
        Assert.Equal(userId, evaluated.UserId);
        Assert.Equal(taskId.Value, evaluated.SpeakingTaskId);
        Assert.Equal(evaluationId.Value, evaluated.SpeakingEvaluationId);
        Assert.Equal(7.0m, evaluated.EstimatedBand);
    }

    [Fact]
    public void AttachEvaluation_on_a_recorded_attempt_fails()
    {
        var attempt = CreateRecorded(out _, out _, out _);

        var result = attempt.AttachEvaluation(SpeakingEvaluationId.From(Guid.NewGuid()), 7.0m, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_SPEAKING_ATTEMPT_NOT_PENDING_EVALUATION", result.Error.Code);
    }

    [Fact]
    public void MarkEvaluationFailed_on_a_submitted_attempt_transitions_to_failed()
    {
        var attempt = CreateRecorded(out _, out _, out _);
        attempt.Submit(Now.AddMinutes(1));

        var result = attempt.MarkEvaluationFailed(Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(SpeakingAttemptStatus.Failed, attempt.Status);
    }
}
