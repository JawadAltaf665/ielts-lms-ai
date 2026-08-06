using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IELTSLms.Abstractions.AI;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI;
using IELTSLms.Application.Exceptions;
using IELTSLms.Application.Learning.Recommendations;
using IELTSLms.Application.Learning.WeaknessDetection;
using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;
using IELTSLms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Learning.StudyPlanner;

/// <summary>
/// The Daily Learning Workflow's per-user pipeline (Sprint 9, Agentic AI Architecture §13.3): Progress
/// Analysis → Learning Recommendation → Study Planner adjust → today's <see cref="DailyPlan"/>. A plain
/// Application-layer sequence, not a runtime multi-agent orchestrator — same explicit simplification
/// Phase 6 Sprints 6-7 used for Writing/Speaking (full orchestrator is Sprint 10). Called both by the
/// <c>DailyLearningWorkflowJob</c> recurring sweep (BackgroundJobs, scheduled) and by
/// <c>GetDailyPlanQueryHandler</c>'s lazy fallback (on the first request of the day if the sweep hasn't
/// run yet) — neither caller decides entitlement/quota here, that happens before this is invoked.
/// </summary>
public sealed partial class DailyLearningWorkflowRunner(
    ProgressAnalysisService progressAnalysis,
    ILearningRecommendationRepository recommendations, IDailyPlanRepository dailyPlans,
    IAiProviderRepository providers, IAiPromptRepository prompts, IAiResponseRepository responses,
    IAiGateway gateway, IUnitOfWork unitOfWork, IClock clock, IGuidProvider guids,
    ILogger<DailyLearningWorkflowRunner> logger)
{
    private const int RecommendationMaxOutputTokens = 768;
    private const int PlannerMaxOutputTokens = 768;

    /// <summary>Default onboarding tasks used when there isn't enough data yet for the AI pipeline
    /// (business rule: don't burn AI budget below the minimum-data threshold) or a step fails — keeps
    /// "never returns empty for an active learner" true even then.</summary>
    private static readonly (LearningSkill Skill, string Title, string Description, int DurationMinutes)[] DefaultTasks =
    [
        (LearningSkill.Grammar, "Complete a Grammar exercise", "Try one Grammar practice exercise to get your first progress signal.", 15),
        (LearningSkill.Reading, "Complete a Reading passage", "Try one Reading passage under timed conditions.", 20),
        (LearningSkill.Vocabulary, "Review 10 vocabulary words", "Review a batch of vocabulary flashcards.", 10),
    ];

    /// <summary>Runs the pipeline for one learner and returns today's plan (existing if already
    /// generated, otherwise freshly generated). Persists everything (weakness profile, recommendation,
    /// plan) in one transaction. Entitlement/quota is the caller's decision, not this method's — pass
    /// <paramref name="allowAiGeneration"/> <see langword="false"/> when the caller has already decided
    /// not to spend AI budget on this user (not entitled, or quota exhausted); this still produces a
    /// default-tasks plan rather than an empty one.</summary>
    public async Task<Result<DailyPlan>> RunForUserAsync(
        Guid tenantId, Guid userId, DateOnly planDate, bool allowAiGeneration, CancellationToken cancellationToken)
    {
        var existingPlan = await dailyPlans.GetByUserAndDateAsync(tenantId, userId, planDate, cancellationToken).ConfigureAwait(false);
        if (existingPlan is not null)
        {
            return Result.Success(existingPlan);
        }

        var now = clock.UtcNow;

        WeaknessProfile? weaknessProfile = null;
        if (allowAiGeneration)
        {
            var weaknessResult = await progressAnalysis.ComputeAsync(tenantId, userId, now, cancellationToken).ConfigureAwait(false);
            weaknessProfile = weaknessResult.IsSuccess ? weaknessResult.Value : null;
            if (weaknessResult.IsFailure)
            {
                WeaknessStepFailed(logger, tenantId, userId, weaknessResult.Error.Code);
            }
        }

        List<DailyPlanItemDto> items;
        AiResponseId? planAiResponseId = null;

        var aiGenerated = weaknessProfile is not null
            ? await TryGenerateAiPlanAsync(tenantId, userId, weaknessProfile, now, cancellationToken).ConfigureAwait(false)
            : null;

        if (aiGenerated is not null)
        {
            (items, planAiResponseId) = aiGenerated.Value;
        }
        else
        {
            items = DefaultTasks
                .Select(t => new DailyPlanItemDto(guids.NewGuid(), t.Skill, t.Title, t.Description, t.DurationMinutes, false, null))
                .ToList();
        }

        var planItemsJson = JsonSerializer.Serialize(items, LearningJsonOptions.Instance);
        var planId = DailyPlanId.From(guids.NewGuid());
        var created = DailyPlan.Create(planId, tenantId, userId, planDate, learningPathId: null, planItemsJson, planAiResponseId, now);
        if (created.IsFailure)
        {
            return Result.Failure<DailyPlan>(created.Error);
        }

        try
        {
            dailyPlans.Add(created.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (UniqueConstraintViolationException)
        {
            // Lost the race against a concurrent generation (scheduled sweep vs lazy fallback) for the
            // same (UserId, PlanDate) — the winner's row is authoritative; hand it back instead of
            // erroring (business rule: the lazy-fallback endpoint must not create a duplicate).
            var winner = await dailyPlans.GetByUserAndDateAsync(tenantId, userId, planDate, cancellationToken).ConfigureAwait(false);
            return winner is not null ? Result.Success(winner) : Result.Failure<DailyPlan>(LearningErrors.DailyPlanGenerationRaceLost);
        }

        return Result.Success(created.Value);
    }

    /// <summary>Steps 2 (Learning Recommendation) and 3 (Study Planner), chained. Returns
    /// <see langword="null"/> on any failure — the caller falls back to <see cref="DefaultTasks"/> rather
    /// than blocking plan generation on an AI hiccup.</summary>
    private async Task<(List<DailyPlanItemDto> Items, AiResponseId? AiResponseId)?> TryGenerateAiPlanAsync(
        Guid tenantId, Guid userId, WeaknessProfile weaknessProfile, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var recommendation = await GenerateRecommendationAsync(tenantId, userId, weaknessProfile, now, cancellationToken).ConfigureAwait(false);
        if (recommendation is null)
        {
            return null;
        }

        return await GeneratePlanItemsAsync(tenantId, recommendation, now, cancellationToken).ConfigureAwait(false);
    }

    private async Task<LearningRecommendation?> GenerateRecommendationAsync(
        Guid tenantId, Guid userId, WeaknessProfile weaknessProfile, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.LearningRecommendation, cancellationToken).ConfigureAwait(false);
        if (provider is null || prompt is null)
        {
            return null;
        }

        var dismissed = await recommendations.GetLatestDismissedAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        var dismissedTitles = dismissed is null
            ? []
            : (Deserialize<List<RecommendationItemDto>>(dismissed.RankedItemsJson) ?? []).Select(i => i.Title).ToList();

        var scores = Deserialize<List<SkillWeaknessScoreDto>>(weaknessProfile.SkillScoresJson) ?? [];
        var inputJson = JsonSerializer.Serialize(
            new { weaknessScores = scores, recentlyDismissedTopics = dismissedTitles }, LearningJsonOptions.Instance);

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.LearningRecommendation, prompt.PromptVersion, provider.Kind, prompt.SystemTemplate,
                inputJson, LearningRecommendationSchema.Name, LearningRecommendationSchema.Json, RecommendationMaxOutputTokens),
            cancellationToken).ConfigureAwait(false);
        if (invocation.IsFailure)
        {
            return null;
        }

        var result = invocation.Value;
        var output = Deserialize<LearningRecommendationOutput>(result.OutputJson);
        if (output is null || output.Items.Count == 0)
        {
            return null;
        }

        var items = output.Items
            .Select(i => (Parsed: Enum.TryParse<LearningSkill>(i.Skill, out var skill), Skill: skill, i.Title, i.Rationale, i.Priority))
            .Where(i => i.Parsed)
            .Select(i => new RecommendationItemDto(i.Skill, i.Title, i.Rationale, i.Priority))
            .ToList();
        if (items.Count == 0)
        {
            return null;
        }

        var recommendationId = LearningRecommendationId.From(guids.NewGuid());
        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(LearningRecommendation), recommendationId.Value, ComputeHash(inputJson), result.Model,
            result.PromptTokens, result.CompletionTokens, result.LatencyMs, result.CostMinorUnits, result.CostCurrency,
            AiResponseStatus.Succeeded, result.OutputJson, result.ProviderResponseId, now);
        if (responseCreate.IsFailure)
        {
            return null;
        }

        var response = responseCreate.Value;
        responses.Add(response);

        var previouslyActive = await recommendations.GetActiveByUserAsync(tenantId, userId, cancellationToken).ConfigureAwait(false);
        _ = previouslyActive?.Supersede(now);

        var rankedItemsJson = JsonSerializer.Serialize(items, LearningJsonOptions.Instance);
        var created = LearningRecommendation.Create(recommendationId, tenantId, userId, rankedItemsJson, response.Id, now);
        if (created.IsFailure)
        {
            return null;
        }

        recommendations.Add(created.Value);
        return created.Value;
    }

    private async Task<(List<DailyPlanItemDto> Items, AiResponseId? AiResponseId)?> GeneratePlanItemsAsync(
        Guid tenantId, LearningRecommendation recommendation, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var provider = await providers.GetActiveByKindAsync(AiProviderKind.Claude, cancellationToken).ConfigureAwait(false);
        var prompt = provider is null
            ? null
            : await prompts.GetActiveForFeatureAsync(tenantId, AiFeatures.StudyPlanner, cancellationToken).ConfigureAwait(false);
        if (provider is null || prompt is null)
        {
            return null;
        }

        var rankedItems = Deserialize<List<RecommendationItemDto>>(recommendation.RankedItemsJson) ?? [];
        var inputJson = JsonSerializer.Serialize(new { recommendations = rankedItems }, LearningJsonOptions.Instance);

        var invocation = await gateway.InvokeAsync(
            new AiGatewayRequest(
                tenantId, AiFeatures.StudyPlanner, prompt.PromptVersion, provider.Kind, prompt.SystemTemplate,
                inputJson, StudyPlannerSchema.Name, StudyPlannerSchema.Json, PlannerMaxOutputTokens),
            cancellationToken).ConfigureAwait(false);
        if (invocation.IsFailure)
        {
            return null;
        }

        var result = invocation.Value;
        var output = Deserialize<StudyPlannerOutput>(result.OutputJson);
        if (output is null || output.Items.Count == 0)
        {
            return null;
        }

        var items = output.Items
            .Select(i => (Parsed: Enum.TryParse<LearningSkill>(i.Skill, out var skill), Skill: skill, i.Title, i.Description, i.DurationMinutes))
            .Where(i => i.Parsed)
            .Select(i => new DailyPlanItemDto(guids.NewGuid(), i.Skill, i.Title, i.Description, i.DurationMinutes, false, null))
            .ToList();
        if (items.Count == 0)
        {
            return null;
        }

        var responseCreate = AiResponse.Create(
            AiResponseId.From(guids.NewGuid()), tenantId, provider.Id, prompt.Id, prompt.PromptVersion,
            nameof(DailyPlan), guids.NewGuid(), ComputeHash(inputJson), result.Model, result.PromptTokens,
            result.CompletionTokens, result.LatencyMs, result.CostMinorUnits, result.CostCurrency,
            AiResponseStatus.Succeeded, result.OutputJson, result.ProviderResponseId, now);
        if (responseCreate.IsFailure)
        {
            return null;
        }

        var response = responseCreate.Value;
        responses.Add(response);

        return (items, response.Id);
    }

    private static string ComputeHash(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static T? Deserialize<T>(string? json) where T : class
    {
        if (json is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, LearningJsonOptions.Instance);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Progress analysis step failed for tenant {TenantId} user {UserId} ({ErrorCode}); falling back to default plan tasks.")]
    private static partial void WeaknessStepFailed(ILogger logger, Guid tenantId, Guid userId, string errorCode);
}
