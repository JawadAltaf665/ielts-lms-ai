using IELTSLms.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Persistence.Seeding;

/// <summary>
/// Seeds an <see cref="AgentRegistration"/> row for every AI agent built in Phase 7 Sprints 4–9
/// (Sprint 10, Agentic AI Architecture §4). Idempotent: uses upsert-by-AgentKey — if the row exists,
/// its descriptors are updated to the current values without changing the key or status.
///
/// Global reference data — no tenant scope. Order is alphabetical by AgentKey within the catalog.
/// The seeder does not set Status; <see cref="AgentRegistration.Register"/> initializes to Enabled.
///
/// Reconciliation note: the 13 agent keys below mirror the <c>AiFeatures.*</c> constants in the
/// Application project (which Persistence cannot reference across the assembly boundary), so they are
/// inlined here as literals — the same pattern as every other literal tag in <see cref="AiCatalogSeeder"/>.
/// </summary>
internal sealed partial class AgentCatalogSeeder(
    IServiceScopeFactory scopeFactory, ILogger<AgentCatalogSeeder> logger) : IHostedService
{
    private static readonly (string Key, string DisplayName, string PromptFeature, string Capabilities, string Tools, string Budget)[] Agents =
    [
        (
            Key: "ai-teacher",
            DisplayName: "AI Teacher",
            PromptFeature: "ai-teacher",
            Capabilities: """{"inputs":["lesson_topic","session_state"],"outputs":["teacher_turn","passed","step"],"skills":["lesson_delivery","comprehension_check","practice_feedback"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":2048,"max_completion_tokens":512}"""
        ),
        (
            Key: "ai-tutor",
            DisplayName: "AI Tutor",
            PromptFeature: "ai-tutor",
            Capabilities: """{"inputs":["learner_question","conversation_history"],"outputs":["tutor_response","in_scope"],"skills":["ielts_qa","strategy_guidance"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":4096,"max_completion_tokens":512}"""
        ),
        (
            Key: "band-predictor",
            DisplayName: "Band Predictor",
            PromptFeature: "band-predictor",
            Capabilities: """{"inputs":["skill_history_summary"],"outputs":["overall_band","reading_band","listening_band","writing_band","speaking_band","confidence"],"skills":["cross_skill_band_prediction"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":2048,"max_completion_tokens":256}"""
        ),
        (
            Key: "grammar-coach",
            DisplayName: "Grammar Coach",
            PromptFeature: "grammar-coach",
            Capabilities: """{"inputs":["learner_text"],"outputs":["corrections"],"skills":["grammar_error_detection","correction_explanation"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":2048,"max_completion_tokens":512}"""
        ),
        (
            Key: "learning-recommendation",
            DisplayName: "Learning Recommendation",
            PromptFeature: "learning-recommendation",
            Capabilities: """{"inputs":["weakness_scores","recently_dismissed_topics"],"outputs":["ranked_recommendations"],"skills":["priority_ranking","study_action_suggestion"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":1024,"max_completion_tokens":768}"""
        ),
        (
            Key: "listening-coach",
            DisplayName: "Listening Coach",
            PromptFeature: "listening-coach",
            Capabilities: """{"inputs":["transcript","wrong_answers","strategy_question"],"outputs":["coaching_response","strategy_tips"],"skills":["wrong_answer_explanation","listening_strategy"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":3072,"max_completion_tokens":768}"""
        ),
        (
            Key: "progress-analysis",
            DisplayName: "Progress Analysis",
            PromptFeature: "progress-analysis",
            Capabilities: """{"inputs":["skill_history"],"outputs":["weakness_scores","summary"],"skills":["cross_skill_weakness_detection","trend_analysis"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":2048,"max_completion_tokens":512}"""
        ),
        (
            Key: "reading-coach",
            DisplayName: "Reading Coach",
            PromptFeature: "reading-coach",
            Capabilities: """{"inputs":["passage","wrong_answers","strategy_question"],"outputs":["coaching_response","strategy_tips"],"skills":["wrong_answer_explanation","reading_strategy"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":4096,"max_completion_tokens":768}"""
        ),
        (
            Key: "sentence-builder-ai",
            DisplayName: "Sentence Builder AI",
            PromptFeature: "sentence-builder-ai",
            Capabilities: """{"inputs":["exercise_prompt","target_pattern","learner_sentence"],"outputs":["matches_pattern","feedback","suggested_revision"],"skills":["sentence_structure_validation","constructive_feedback"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":1024,"max_completion_tokens":256}"""
        ),
        (
            Key: "speaking-examiner",
            DisplayName: "Speaking Examiner",
            PromptFeature: "speaking-examiner",
            Capabilities: """{"inputs":["transcript","task","rubric"],"outputs":["fluency_band","lexical_band","grammar_band","pronunciation_band","estimated_band","confidence","requires_review","feedback"],"skills":["ielts_speaking_scoring","rubric_application"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":4096,"max_completion_tokens":768}"""
        ),
        (
            Key: "study-planner",
            DisplayName: "Study Planner",
            PromptFeature: "study-planner",
            Capabilities: """{"inputs":["recommendations"],"outputs":["daily_plan_items"],"skills":["daily_schedule_generation","time_allocation"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":1024,"max_completion_tokens":768}"""
        ),
        (
            Key: "vocabulary-coach",
            DisplayName: "Vocabulary Coach",
            PromptFeature: "vocabulary-coach",
            Capabilities: """{"inputs":["word","topic","difficulty"],"outputs":["usage_examples","collocations","synonyms"],"skills":["contextual_usage","collocation_analysis","synonym_suggestion"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":1024,"max_completion_tokens":512}"""
        ),
        (
            Key: "writing-examiner",
            DisplayName: "Writing Examiner",
            PromptFeature: "writing-examiner",
            Capabilities: """{"inputs":["task","submission","rubric"],"outputs":["task_achievement_band","coherence_band","lexical_band","grammar_band","estimated_band","confidence","requires_review","feedback","similarity_flag"],"skills":["ielts_writing_scoring","rubric_application","similarity_detection"]}""",
            Tools: """{}""",
            Budget: """{"max_prompt_tokens":4096,"max_completion_tokens":1024}"""
        ),
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            foreach (var agent in Agents)
            {
                await UpsertAgentAsync(dbContext, agent.Key, agent.DisplayName, agent.PromptFeature,
                    agent.Capabilities, agent.Tools, agent.Budget, now, cancellationToken).ConfigureAwait(false);
            }

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            AgentCatalogSeeded(logger, Agents.Length);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SeedingFailed(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task UpsertAgentAsync(
        ApplicationDbContext dbContext,
        string agentKey,
        string displayName,
        string promptFeature,
        string capabilitiesJson,
        string toolsJson,
        string budgetJson,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters: AgentRegistration is not tenant-scoped, but the soft-delete global
        // filter still applies from ApplicationDbContext — use IgnoreQueryFilters to see all rows
        // including previously soft-deleted ones, so we can detect any existing record.
        var existing = await dbContext.Set<AgentRegistration>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.AgentKey == agentKey, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            // Idempotent update: keep key and status, refresh descriptors so metadata stays current.
            existing.UpdateDescriptors(displayName, capabilitiesJson, promptFeature, toolsJson, budgetJson, now);
            return;
        }

        var created = AgentRegistration.Register(
            AgentRegistrationId.From(Guid.NewGuid()),
            agentKey,
            displayName,
            capabilitiesJson,
            promptFeature,
            toolsJson,
            budgetJson,
            now);

        if (created.IsSuccess)
        {
            dbContext.Set<AgentRegistration>().Add(created.Value);
        }
    }

    [LoggerMessage(EventId = 4020, Level = LogLevel.Information,
        Message = "Agent catalog seed complete ({Count} agents registered/updated).")]
    private static partial void AgentCatalogSeeded(ILogger logger, int count);

    [LoggerMessage(EventId = 4021, Level = LogLevel.Warning,
        Message = "Failed to seed the agent catalog; the database may be unavailable. Seeding will retry on next startup.")]
    private static partial void SeedingFailed(ILogger logger, Exception exception);
}
