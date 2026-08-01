using IELTSLms.Domain.Writing;
using IELTSLms.Domain.Writing.Events;

namespace IELTSLms.Domain.Tests.Writing;

public sealed class WritingSubmissionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static WritingSubmission CreateDraft(out Guid tenantId, out Guid userId, out WritingTaskId taskId)
    {
        tenantId = Guid.NewGuid();
        userId = Guid.NewGuid();
        taskId = WritingTaskId.From(Guid.NewGuid());
        return WritingSubmission.Create(WritingSubmissionId.From(Guid.NewGuid()), tenantId, userId, taskId, "My essay text.", Now).Value;
    }

    [Fact]
    public void Create_starts_in_draft_with_no_domain_events()
    {
        var submission = CreateDraft(out _, out _, out _);

        Assert.Equal(WritingSubmissionStatus.Draft, submission.Status);
        Assert.Empty(submission.DomainEvents);
    }

    [Fact]
    public void Create_with_empty_response_text_fails()
    {
        var result = WritingSubmission.Create(
            WritingSubmissionId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), WritingTaskId.From(Guid.NewGuid()), "  ", Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_WRITING_RESPONSE_TEXT_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void Submit_transitions_to_submitted_and_raises_WritingSubmitted()
    {
        var submission = CreateDraft(out var tenantId, out var userId, out var taskId);

        var result = submission.Submit(250, Now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(WritingSubmissionStatus.Submitted, submission.Status);
        Assert.Equal(250, submission.WordCount);
        Assert.Equal(Now.AddMinutes(1), submission.SubmittedAtUtc);

        var raised = Assert.Single(submission.DomainEvents);
        var submitted = Assert.IsType<WritingSubmittedDomainEvent>(raised);
        Assert.Equal(tenantId, submitted.TenantId);
        Assert.Equal(userId, submitted.UserId);
        Assert.Equal(taskId.Value, submitted.WritingTaskId);
        Assert.Equal(submission.Id.Value, submitted.WritingSubmissionId);
    }

    [Fact]
    public void Submitting_an_already_submitted_submission_fails_rather_than_resubmitting()
    {
        var submission = CreateDraft(out _, out _, out _);
        submission.Submit(250, Now.AddMinutes(1));

        var second = submission.Submit(300, Now.AddMinutes(2));

        Assert.True(second.IsFailure);
        Assert.Equal("CONFLICT_WRITING_SUBMISSION_ALREADY_SUBMITTED", second.Error.Code);
        Assert.Equal(250, submission.WordCount);
    }

    [Fact]
    public void AttachEvaluation_on_a_submitted_submission_transitions_to_evaluated_and_raises_WritingEvaluated()
    {
        var submission = CreateDraft(out var tenantId, out var userId, out var taskId);
        submission.Submit(250, Now.AddMinutes(1));
        var evaluationId = WritingEvaluationId.From(Guid.NewGuid());

        var result = submission.AttachEvaluation(evaluationId, 7.0m, Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(WritingSubmissionStatus.Evaluated, submission.Status);
        Assert.Equal(evaluationId, submission.WritingEvaluationId);

        var raised = Assert.Single(submission.DomainEvents, e => e is WritingEvaluatedDomainEvent);
        var evaluated = Assert.IsType<WritingEvaluatedDomainEvent>(raised);
        Assert.Equal(tenantId, evaluated.TenantId);
        Assert.Equal(userId, evaluated.UserId);
        Assert.Equal(taskId.Value, evaluated.WritingTaskId);
        Assert.Equal(evaluationId.Value, evaluated.WritingEvaluationId);
        Assert.Equal(7.0m, evaluated.EstimatedBand);
    }

    [Fact]
    public void AttachEvaluation_on_a_draft_submission_fails()
    {
        var submission = CreateDraft(out _, out _, out _);

        var result = submission.AttachEvaluation(WritingEvaluationId.From(Guid.NewGuid()), 7.0m, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_WRITING_SUBMISSION_NOT_PENDING_EVALUATION", result.Error.Code);
    }

    [Fact]
    public void MarkEvaluationFailed_on_a_submitted_submission_transitions_to_failed()
    {
        var submission = CreateDraft(out _, out _, out _);
        submission.Submit(250, Now.AddMinutes(1));

        var result = submission.MarkEvaluationFailed(Now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(WritingSubmissionStatus.Failed, submission.Status);
    }
}
