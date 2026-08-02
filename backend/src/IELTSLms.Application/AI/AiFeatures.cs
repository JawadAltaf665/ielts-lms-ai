using IELTSLms.Domain.AI;

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

    /// <summary>
    /// <see cref="Domain.AI.AiPrompt.Feature"/> tag for conversation-history compaction (Phase 7 Sprint 3,
    /// AI Architecture §6). Triggered internally by <c>IConversationContextService</c> when a
    /// conversation's raw tail exceeds its token budget — not a directly learner-invoked capability, so
    /// unlike the entries above it has no entitlement/quota gating of its own.
    /// </summary>
    public const string ConversationSummary = "conversation-summary";

    /// <summary>
    /// <see cref="Domain.AI.AiPrompt.Feature"/> tag, <see cref="Domain.AI.Conversation.Feature"/> tag,
    /// and <see cref="Domain.AI.AiOperation.Kind"/> value for AI Tutor (Phase 7 Sprint 4,
    /// Agentic AI Architecture §12.1). Same literal also names the gating Billing
    /// <c>Feature.Code</c> — reconciliation notes flag this single-tag convention as new (Grammar's
    /// entitlement code carries an <c>"ai."</c> prefix its capability tag doesn't).
    /// </summary>
    public const string AiTutor = "ai-tutor";

    /// <summary>Billing <c>QuotaDefinition.Code</c> metering AI Tutor "ask" turns.</summary>
    public const string AiTutorQuota = "ai-tutor.requests";

    /// <summary>
    /// <see cref="Domain.AI.AiPrompt.Feature"/> tag, <see cref="Domain.AI.Conversation.Feature"/> tag,
    /// and <see cref="Domain.AI.AiOperation.Kind"/> value for AI Teacher (Phase 7 Sprint 4). Also the
    /// gating Billing <c>Feature.Code</c> — see <see cref="AiTutor"/>'s remarks. Per-lesson-step prompt
    /// variants live under this one feature via <see cref="Domain.AI.AiPrompt.Scope"/>
    /// (see <see cref="LessonStepScopes"/>), not four separate feature tags.
    /// </summary>
    public const string AiTeacher = "ai-teacher";

    /// <summary>Billing <c>QuotaDefinition.Code</c> metering AI Teacher turns (session start + each respond).</summary>
    public const string AiTeacherQuota = "ai-teacher.requests";
}

/// <summary>
/// <see cref="Domain.AI.AiPrompt.Scope"/> values for the <see cref="AiFeatures.AiTeacher"/> feature —
/// one active prompt per <see cref="Domain.AI.LessonStep"/>, resolved by the step-state-machine rather
/// than a single fixed template (this sprint's whole reason for using the Scope slot per-feature —
/// see AiPrompt's own doc comment on "(TenantId, Feature, Scope) slot").
/// </summary>
internal static class LessonStepScopes
{
    public const string Explain = "explain";
    public const string CheckUnderstanding = "check-understanding";
    public const string Practice = "practice";
    public const string Recap = "recap";

    public static string ForStep(LessonStep step) => step switch
    {
        LessonStep.Explain => Explain,
        LessonStep.CheckUnderstanding => CheckUnderstanding,
        LessonStep.Practice => Practice,
        LessonStep.Recap => Recap,
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Completed has no prompt scope."),
    };
}
