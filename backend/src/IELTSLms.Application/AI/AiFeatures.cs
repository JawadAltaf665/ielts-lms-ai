namespace IELTSLms.Application.AI;

/// <summary>Well-known codes tying the AI capability catalog together: the <c>AiPrompt.Feature</c> tag,
/// the Billing <c>Feature.Code</c> entitlement, and the Billing <c>QuotaDefinition.Code</c> — seeded
/// together by <c>AiCatalogSeeder</c> (Persistence).</summary>
internal static class AiFeatures
{
    /// <summary><see cref="Domain.AI.AiPrompt.Feature"/> tag and <see cref="Domain.AI.AiOperation.Kind"/> value.</summary>
    public const string GrammarCoach = "grammar-coach";

    /// <summary>Billing <c>Feature.Code</c> gating access to Grammar Coach.</summary>
    public const string GrammarCoachEntitlement = "ai.grammar-coach";

    /// <summary>Billing <c>QuotaDefinition.Code</c> metering Grammar Coach calls.</summary>
    public const string GrammarCoachQuota = "ai.grammar-coach.requests";
}
