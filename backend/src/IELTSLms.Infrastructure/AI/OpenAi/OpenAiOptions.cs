namespace IELTSLms.Infrastructure.AI.OpenAi;

/// <summary>
/// Binds the <c>Ai:OpenAi</c> configuration section. Not <c>ValidateOnStart()</c>, mirroring
/// <see cref="Claude.ClaudeOptions"/> — a blank API key must not prevent the whole API from starting in
/// local dev or CI; absence is caught at first use instead.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "Ai:OpenAi";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.openai.com";

    public string Model { get; set; } = "gpt-4o-mini";

    // ponytail: static configured per-million-token rates, not a live provider billing/pricing API —
    // same tradeoff as ClaudeOptions; upgrade path is fetching real cost from the provider's usage API
    // if per-call accuracy matters.
    public decimal InputCostPerMillionTokensUsd { get; set; }

    public decimal OutputCostPerMillionTokensUsd { get; set; }
}
