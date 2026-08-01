using System.Text.Json;
using IELTSLms.Domain.Listening.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Listening;

/// <summary>
/// A learner's timed attempt at a <see cref="ListeningAudio"/> (ERD ENT-026). Reuses
/// <c>ReadingAttempt</c>'s two-phase server-timer template (Phase 6 Sprint 4): <see cref="Start"/>
/// stamps the server-owned deadline, <see cref="Submit"/> grades and closes it. Adds what Reading
/// didn't need: <see cref="Mode"/> (drives the replay cap via <see cref="ListeningReplayPolicy"/> and
/// transcript visibility) and server-tracked <see cref="ReplayCount"/>/<see cref="PlaybackEventsJson"/>
/// — Sprint 5's playback-rule enforcement. Never trust a client-reported replay count:
/// <see cref="RegisterReplay"/> is the sole place the cap is checked and incremented.
/// </summary>
public sealed class ListeningAttempt : TenantAuditableEntity<ListeningAttemptId>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private ListeningAttempt(
        ListeningAttemptId id, Guid tenantId, Guid userId, ListeningAudioId audioId, ListeningMode mode,
        DateTimeOffset startedAtUtc, DateTimeOffset deadlineAtUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        ListeningAudioId = audioId;
        Mode = mode;
        StartedAtUtc = startedAtUtc;
        DeadlineAtUtc = deadlineAtUtc;
        ReplayCount = 0;
        PlaybackEventsJson = JsonSerializer.Serialize(new[] { new PlaybackEvent("play", startedAtUtc) }, JsonOptions);
        CreatedAtUtc = startedAtUtc;
    }

    private ListeningAttempt()
    {
        PlaybackEventsJson = "[]";
    }

    public Guid UserId { get; private init; }

    public ListeningAudioId ListeningAudioId { get; private init; }

    /// <summary>Stamped at <see cref="Start"/>; governs the replay cap and transcript visibility for the life of this attempt.</summary>
    public ListeningMode Mode { get; private init; }

    /// <summary>Server clock at attempt start.</summary>
    public DateTimeOffset StartedAtUtc { get; private init; }

    /// <summary>Server-computed deadline (<see cref="StartedAtUtc"/> + the audio's configured duration). Never client-supplied.</summary>
    public DateTimeOffset DeadlineAtUtc { get; private init; }

    public DateTimeOffset? SubmittedAtUtc { get; private set; }

    /// <summary>JSON array of <c>{ questionId, submittedAnswer, isCorrect }</c> per question. Null until submitted.</summary>
    public string? AnswersJson { get; private set; }

    public int? RawScore { get; private set; }

    public int? MaxScore { get; private set; }

    public decimal? Band { get; private set; }

    /// <summary>Replays registered so far via <see cref="RegisterReplay"/> — never client-supplied.</summary>
    public int ReplayCount { get; private set; }

    /// <summary>JSON array of timestamped playback events — the audit trail for the replay-count rule.</summary>
    public string PlaybackEventsJson { get; private set; }

    public bool IsSubmitted => SubmittedAtUtc is not null;

    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc > DeadlineAtUtc;

    public static ListeningAttempt Start(
        ListeningAttemptId id, Guid tenantId, Guid userId, ListeningAudioId audioId, ListeningMode mode,
        DateTimeOffset nowUtc, int durationSeconds) =>
        new(id, tenantId, userId, audioId, mode, nowUtc, nowUtc.AddSeconds(durationSeconds));

    /// <summary>
    /// Registers one replay against <see cref="Mode"/>'s server-tracked cap
    /// (<see cref="ListeningReplayPolicy"/>) — the sole place this sprint's playback rule is enforced.
    /// </summary>
    public Result RegisterReplay(DateTimeOffset nowUtc)
    {
        if (IsSubmitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_LISTENING_ATTEMPT_ALREADY_SUBMITTED", "This attempt has already been submitted."));
        }

        if (ReplayCount >= ListeningReplayPolicy.AllowedReplays(Mode))
        {
            return Result.Failure(Error.Conflict(
                "BUSINESS_REPLAY_LIMIT_EXCEEDED", "The allowed number of replays for this attempt has been reached."));
        }

        ReplayCount++;

        var events = JsonSerializer.Deserialize<List<PlaybackEvent>>(PlaybackEventsJson, JsonOptions) ?? [];
        events.Add(new PlaybackEvent("replay", nowUtc));
        PlaybackEventsJson = JsonSerializer.Serialize(events, JsonOptions);
        UpdatedAtUtc = nowUtc;

        return Result.Success();
    }

    /// <summary>
    /// Grades and closes the attempt. Idempotent by construction: <see cref="IsSubmitted"/> attempts
    /// refuse a second scoring pass — callers should treat an already-submitted attempt as success and
    /// return the stored result rather than call this again (ERD §4.3 concurrency strategy).
    /// </summary>
    public Result Submit(string answersJson, int rawScore, int maxScore, decimal band, DateTimeOffset nowUtc)
    {
        if (IsSubmitted)
        {
            return Result.Failure(Error.Conflict(
                "CONFLICT_LISTENING_ATTEMPT_ALREADY_SUBMITTED", "This attempt has already been submitted."));
        }

        AnswersJson = answersJson;
        RawScore = rawScore;
        MaxScore = maxScore;
        Band = band;
        SubmittedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;

        RaiseDomainEvent(new ListeningAttemptScoredDomainEvent(TenantId, UserId, ListeningAudioId.Value, Id.Value, rawScore, maxScore, band));
        return Result.Success();
    }

    private sealed record PlaybackEvent(string Event, DateTimeOffset AtUtc);
}
