using IELTSLms.Domain.Reading;
using IELTSLms.Domain.Reading.Events;

namespace IELTSLms.Domain.Tests.Reading;

public sealed class ReadingAttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_stamps_the_server_owned_deadline_from_the_passage_duration()
    {
        var attempt = ReadingAttempt.Start(
            ReadingAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ReadingPassageId.From(Guid.NewGuid()), Now, durationSeconds: 3600);

        Assert.Equal(Now, attempt.StartedAtUtc);
        Assert.Equal(Now.AddSeconds(3600), attempt.DeadlineAtUtc);
        Assert.False(attempt.IsSubmitted);
        Assert.Empty(attempt.DomainEvents);
    }

    [Fact]
    public void IsExpired_is_true_once_now_passes_the_deadline()
    {
        var attempt = ReadingAttempt.Start(
            ReadingAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ReadingPassageId.From(Guid.NewGuid()), Now, durationSeconds: 60);

        Assert.False(attempt.IsExpired(Now.AddSeconds(60)));
        Assert.True(attempt.IsExpired(Now.AddSeconds(61)));
    }

    [Fact]
    public void Submit_records_the_score_and_raises_ReadingAttemptScored()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var passageId = ReadingPassageId.From(Guid.NewGuid());
        var attempt = ReadingAttempt.Start(ReadingAttemptId.From(Guid.NewGuid()), tenantId, userId, passageId, Now, 1800);
        var submittedAt = Now.AddMinutes(10);

        var result = attempt.Submit("[]", rawScore: 30, maxScore: 40, band: 7.0m, submittedAt);

        Assert.True(result.IsSuccess);
        Assert.True(attempt.IsSubmitted);
        Assert.Equal(30, attempt.RawScore);
        Assert.Equal(40, attempt.MaxScore);
        Assert.Equal(7.0m, attempt.Band);
        Assert.Equal(submittedAt, attempt.SubmittedAtUtc);

        var raised = Assert.Single(attempt.DomainEvents);
        var scored = Assert.IsType<ReadingAttemptScoredDomainEvent>(raised);
        Assert.Equal(tenantId, scored.TenantId);
        Assert.Equal(userId, scored.UserId);
        Assert.Equal(passageId.Value, scored.ReadingPassageId);
        Assert.Equal(attempt.Id.Value, scored.ReadingAttemptId);
        Assert.Equal(30, scored.RawScore);
        Assert.Equal(40, scored.MaxScore);
        Assert.Equal(7.0m, scored.Band);
    }

    [Fact]
    public void Submitting_an_already_submitted_attempt_fails_rather_than_rescoring()
    {
        var attempt = ReadingAttempt.Start(
            ReadingAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ReadingPassageId.From(Guid.NewGuid()), Now, 1800);
        attempt.Submit("[]", 10, 20, 5.0m, Now.AddMinutes(5));

        var second = attempt.Submit("[]", 20, 20, 9.0m, Now.AddMinutes(6));

        Assert.True(second.IsFailure);
        Assert.Equal("CONFLICT_READING_ATTEMPT_ALREADY_SUBMITTED", second.Error.Code);
        Assert.Equal(10, attempt.RawScore);
    }
}
