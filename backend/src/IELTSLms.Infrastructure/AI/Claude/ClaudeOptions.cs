namespace IELTSLms.Infrastructure.AI.Claude;

/// <summary>
/// Binds the <c>Ai:Claude</c> configuration section. Deliberately not <c>ValidateOnStart()</c> — same
/// reasoning as <see cref="Billing.StripeOptions"/>: a blank API key must not prevent the whole API
/// from starting in local dev or CI; absence is caught at first use instead.
/// </summary>
public sealed class ClaudeOptions
{
    public const string SectionName = "Ai:Claude";

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.anthropic.com";

    public string Model { get; set; } = "claude-3-5-haiku-20241022";

    public string AnthropicVersion { get; set; } = "2023-06-01";

    // ponytail: static configured per-million-token rates, not a live provider billing/pricing API —
    // upgrade path is fetching real cost from the provider's usage API if per-call accuracy matters.
    public decimal InputCostPerMillionTokensUsd { get; set; }

    public decimal OutputCostPerMillionTokensUsd { get; set; }
}
