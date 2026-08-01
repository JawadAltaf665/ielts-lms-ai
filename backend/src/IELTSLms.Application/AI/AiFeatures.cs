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

    /// <summary>
    /// <see cref="Domain.AI.AiPrompt.Feature"/> tag for the Writing Examiner step (Phase 6 Sprint 6,
    /// Agentic AI Architecture §13.1). The Grammar Agent step of that same workflow reuses
    /// <see cref="GrammarCoach"/> above rather than a second prompt.
    /// </summary>
    public const string WritingExaminer = "writing-examiner";

    /// <summary><see cref="Domain.AI.AiPrompt.Feature"/> tag for the Writing workflow's Vocabulary
    /// Agent step — no existing capability to reuse, unlike the Grammar Agent step.</summary>
    public const string VocabularyAnalysis = "vocabulary-analysis";

    /// <summary>Billing <c>Feature.Code</c> gating the whole Writing Evaluation Workflow (all three
    /// AI calls it makes) as a single entitlement, not gated per sub-step.</summary>
    public const string WritingEvaluationEntitlement = "ai.writing-evaluation";

    /// <summary>Billing <c>QuotaDefinition.Code</c> metering Writing evaluation requests (one unit per submission).</summary>
    public const string WritingEvaluationQuota = "ai.writing-evaluation.requests";

    /// <summary>
    /// <see cref="Domain.AI.AiPrompt.Feature"/> tag for the Speaking Examiner step (Phase 6 Sprint 7,
    /// Agentic AI Architecture §13.2). Unlike Writing, Speaking's workflow has a single agent — no
    /// separate grammar/vocabulary sub-steps.
    /// </summary>
    public const string SpeakingExaminer = "speaking-examiner";

    /// <summary>Billing <c>Feature.Code</c> gating the Speaking Evaluation Workflow (transcription + examiner scoring).</summary>
    public const string SpeakingEvaluationEntitlement = "ai.speaking-evaluation";

    /// <summary>Billing <c>QuotaDefinition.Code</c> metering Speaking evaluation requests (one unit per attempt).</summary>
    public const string SpeakingEvaluationQuota = "ai.speaking-evaluation.requests";

    /// <summary>
    /// <see cref="Domain.AI.AiPrompt.Feature"/> tag for the standalone, history-based Band Predictor
    /// (Phase 6 Sprint 9, Agentic AI Architecture §13.6) — a new capability, not a reuse of the
    /// per-submission Writing/Speaking Examiner <c>EstimatedBand</c> values.
    /// </summary>
    public const string BandPredictor = "band-predictor";

    /// <summary>Billing <c>Feature.Code</c> gating the Band Prediction Workflow.</summary>
    public const string BandPredictorEntitlement = "ai.band-predictor";

    /// <summary>Billing <c>QuotaDefinition.Code</c> metering Band Prediction requests (one unit per request).</summary>
    public const string BandPredictorQuota = "ai.band-predictor.requests";
}
