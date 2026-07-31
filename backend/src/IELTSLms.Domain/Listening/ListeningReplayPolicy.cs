namespace IELTSLms.Domain.Listening;

/// <summary>
/// The confirmed replay-count policy per mode (Sprint 5 business rules, CB-IELTS convention): exam
/// mode is single-play (zero replays beyond the initial listen served by
/// <c>GET /listening-audios/{id}/stream</c>); learning mode is unlimited. Server-tracked only —
/// <see cref="ListeningAttempt.RegisterReplay"/> is the sole place this cap is enforced, never the client.
/// </summary>
public static class ListeningReplayPolicy
{
    public static int AllowedReplays(ListeningMode mode) => mode switch
    {
        ListeningMode.Exam => 0,
        ListeningMode.Learning => int.MaxValue,
        _ => 0,
    };
}
