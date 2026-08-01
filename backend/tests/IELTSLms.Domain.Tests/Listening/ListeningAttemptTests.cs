using IELTSLms.Domain.Listening;
using IELTSLms.Domain.Listening.Events;

namespace IELTSLms.Domain.Tests.Listening;

public sealed class ListeningAttemptTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Start_stamps_the_server_owned_deadline_from_the_audios_configured_duration()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Exam, Now, durationSeconds: 1800);

        Assert.Equal(Now, attempt.StartedAtUtc);
        Assert.Equal(Now.AddSeconds(1800), attempt.DeadlineAtUtc);
        Assert.False(attempt.IsSubmitted);
        Assert.Equal(0, attempt.ReplayCount);
        Assert.Empty(attempt.DomainEvents);
    }

    [Fact]
    public void IsExpired_is_true_once_now_passes_the_deadline()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Exam, Now, durationSeconds: 60);

        Assert.False(attempt.IsExpired(Now.AddSeconds(60)));
        Assert.True(attempt.IsExpired(Now.AddSeconds(61)));
    }

    [Fact]
    public void Exam_mode_rejects_any_replay_with_the_business_replay_limit_code()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Exam, Now, durationSeconds: 1800);

        var result = attempt.RegisterReplay(Now.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal("BUSINESS_REPLAY_LIMIT_EXCEEDED", result.Error.Code);
        Assert.Equal(0, attempt.ReplayCount);
    }

    [Fact]
    public void Learning_mode_allows_many_replays()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Learning, Now, durationSeconds: 1800);

        for (var i = 1; i <= 10; i++)
        {
            var result = attempt.RegisterReplay(Now.AddMinutes(i));
            Assert.True(result.IsSuccess);
            Assert.Equal(i, attempt.ReplayCount);
        }
    }

    [Fact]
    public void RegisterReplay_records_a_timestamped_playback_event()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Learning, Now, durationSeconds: 1800);

        attempt.RegisterReplay(Now.AddMinutes(1));

        Assert.Contains("\"play\"", attempt.PlaybackEventsJson);
        Assert.Contains("\"replay\"", attempt.PlaybackEventsJson);
    }

    [Fact]
    public void RegisterReplay_on_an_already_submitted_attempt_fails()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Learning, Now, durationSeconds: 1800);
        attempt.Submit("[]", 5, 10, 5.0m, Now.AddMinutes(5));

        var result = attempt.RegisterReplay(Now.AddMinutes(6));

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_LISTENING_ATTEMPT_ALREADY_SUBMITTED", result.Error.Code);
    }

    [Fact]
    public void Submit_records_the_score_and_raises_ListeningAttemptScored()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var audioId = ListeningAudioId.From(Guid.NewGuid());
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), tenantId, userId, audioId, ListeningMode.Exam, Now, 1800);
        var submittedAt = Now.AddMinutes(10);

        var result = attempt.Submit("[]", rawScore: 30, maxScore: 40, band: 7.0m, submittedAt);

        Assert.True(result.IsSuccess);
        Assert.True(attempt.IsSubmitted);
        Assert.Equal(30, attempt.RawScore);
        Assert.Equal(40, attempt.MaxScore);
        Assert.Equal(7.0m, attempt.Band);
        Assert.Equal(submittedAt, attempt.SubmittedAtUtc);

        var raised = Assert.Single(attempt.DomainEvents);
        var scored = Assert.IsType<ListeningAttemptScoredDomainEvent>(raised);
        Assert.Equal(tenantId, scored.TenantId);
        Assert.Equal(userId, scored.UserId);
        Assert.Equal(audioId.Value, scored.ListeningAudioId);
        Assert.Equal(attempt.Id.Value, scored.ListeningAttemptId);
        Assert.Equal(30, scored.RawScore);
        Assert.Equal(40, scored.MaxScore);
        Assert.Equal(7.0m, scored.Band);
    }

    [Fact]
    public void Submitting_an_already_submitted_attempt_fails_rather_than_rescoring()
    {
        var attempt = ListeningAttempt.Start(
            ListeningAttemptId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), ListeningAudioId.From(Guid.NewGuid()),
            ListeningMode.Exam, Now, 1800);
        attempt.Submit("[]", 10, 20, 5.0m, Now.AddMinutes(5));

        var second = attempt.Submit("[]", 20, 20, 9.0m, Now.AddMinutes(6));

        Assert.True(second.IsFailure);
        Assert.Equal("CONFLICT_LISTENING_ATTEMPT_ALREADY_SUBMITTED", second.Error.Code);
        Assert.Equal(10, attempt.RawScore);
    }
}
