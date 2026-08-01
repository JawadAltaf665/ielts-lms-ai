namespace IELTSLms.Infrastructure.AI.Whisper;

/// <summary>
/// Binds the <c>Ai:Whisper</c> configuration section. Deliberately not <c>ValidateOnStart()</c> — same
/// reasoning as <see cref="Claude.ClaudeOptions"/>: a blank API key must not prevent the whole API from
/// starting in local dev or CI; absence is caught at first use instead.
/// </summary>
public sealed class WhisperOptions
{
    public const string SectionName = "Ai:Whisper";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com";

    public string Model { get; set; } = "whisper-1";
}
