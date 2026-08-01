namespace IELTSLms.Application.Listening;

/// <summary>Type/size allowlist for listening audio uploads (ERD ENT-057 business rule), fed into <c>FileAsset.Create</c>.</summary>
internal static class ListeningAudioFilePolicy
{
    public static readonly string[] AllowedContentTypes =
    [
        "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/mp4", "audio/aac", "audio/ogg",
    ];

    public const long MaxSizeBytes = 50 * 1024 * 1024;
}
