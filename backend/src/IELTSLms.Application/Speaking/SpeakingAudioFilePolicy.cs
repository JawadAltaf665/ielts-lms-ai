namespace IELTSLms.Application.Speaking;

/// <summary>Type/size allowlist for speaking recording uploads (ERD ENT-057 business rule), fed into
/// <c>FileAsset.Create</c>. Same allowlist as <c>ListeningAudioFilePolicy</c> — both are learner-facing
/// spoken-audio uploads.</summary>
internal static class SpeakingAudioFilePolicy
{
    public static readonly string[] AllowedContentTypes =
    [
        "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/mp4", "audio/aac", "audio/ogg", "audio/webm",
    ];

    public const long MaxSizeBytes = 50 * 1024 * 1024;

    /// <summary>Shortest recording accepted before it's rejected as too-short to evaluate (ERD ENT-031
    /// business rule) — checked at submit time, not upload time, per the sprint's "fail before it hits
    /// the STT pipeline" rule.</summary>
    public const int MinDurationSeconds = 5;
}
