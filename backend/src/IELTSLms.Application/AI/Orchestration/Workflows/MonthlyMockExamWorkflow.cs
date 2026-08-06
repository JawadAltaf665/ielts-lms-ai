using System.Text.Json;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI.RequestBandPrediction;
using IELTSLms.Application.Learning.StudyPlanner;
using MediatR;

namespace IELTSLms.Application.AI.Orchestration.Workflows;

/// <summary>
/// Well-known context keys used by the Monthly Mock Exam Workflow steps (Sprint 10). Centralised here
/// so the workflow definition and the command handler share the same string constants without an
/// implicit coupling between their namespaces.
/// </summary>
public static class MonthlyMockExamWorkflowKeys
{
    public const string MockExamId = "mock_exam_id";
    public const string ReadingBand = "reading_band";
    public const string ListeningBand = "listening_band";
    public const string WritingBand = "writing_band";
    public const string SpeakingBand = "speaking_band";
    public const string OverallBand = "overall_band";
    public const string BandPredictionId = "band_prediction_id";
    public const string LearningRecommendationGenerated = "learning_recommendation_generated";
}

/// <summary>
/// The concrete <see cref="WorkflowDefinition"/> for the Monthly Mock Exam Orchestration
/// (Sprint 10, Agentic AI Architecture §7 "Monthly Mock Exam Workflow").
///
/// Step design:
/// <list type="number">
///   <item><b>Parallel:</b> ReadingScore + ListeningScore — pure reads from context (no AI),
///     written so the engine records them in the trace for observability.</item>
///   <item><b>Sequential:</b> BandPredictor — enqueues <see cref="RequestBandPredictionCommand"/>
///     via MediatR. Returns immediately (async job); the step records the enqueued operation id.</item>
///   <item><b>Conditional:</b> LearningRecommendation — runs the
///     <see cref="DailyLearningWorkflowRunner"/> for today's plan if the band predictor step
///     succeeded. Skipped on band predictor failure so the exam result is still visible.</item>
/// </list>
///
/// All three retry counts are 0 (no additional retries at the workflow layer) because each sub-step
/// already uses the AI Gateway's own exponential-backoff retry, and double-retrying band prediction
/// would double-charge quota.
/// </summary>
public static class MonthlyMockExamWorkflow
{
    /// <summary>Stable slug for this workflow type.</summary>
    public const string Key = "monthly-mock-exam";

    /// <summary>Builds the <see cref="WorkflowDefinition"/> wiring the three step groups together.
    /// Called once per execution — the definition is lightweight (all closures capture only
    /// tenant-scoped services, not shared state).</summary>
    public static WorkflowDefinition Build(
        Guid tenantId,
        Guid userId,
        ISender sender,
        DailyLearningWorkflowRunner learningRunner,
        IClock clock)
    {
        return new WorkflowDefinition(Key,
        [
            // Step 1a (Parallel): Record Reading band from context (already scored by section job).
            new StepDefinition(
                StepKey: "reading-score",
                Kind: StepKind.Parallel,
                Execute: (ctx, ct) =>
                {
                    var band = ctx.Get<decimal?>(MonthlyMockExamWorkflowKeys.ReadingBand);
                    var output = JsonSerializer.Serialize(new { reading_band = band });
                    return Task.FromResult(StepResult.Success(output));
                }),

            // Step 1b (Parallel): Record Listening band from context.
            new StepDefinition(
                StepKey: "listening-score",
                Kind: StepKind.Parallel,
                Execute: (ctx, ct) =>
                {
                    var band = ctx.Get<decimal?>(MonthlyMockExamWorkflowKeys.ListeningBand);
                    var output = JsonSerializer.Serialize(new { listening_band = band });
                    return Task.FromResult(StepResult.Success(output));
                }),

            // Step 2 (Sequential): Enqueue band prediction for the learner.
            // Uses MediatR to dispatch RequestBandPredictionCommand — the handler enforces
            // entitlement/quota and enqueues a background job. We don't await the job result here;
            // the orchestrated-review response carries the WorkflowRun + its step trace.
            new StepDefinition(
                StepKey: "band-predictor",
                Kind: StepKind.Sequential,
                Execute: async (ctx, ct) =>
                {
                    var command = new RequestBandPredictionCommand(tenantId, userId);
                    var result = await sender.Send(command, ct).ConfigureAwait(false);

                    if (result.IsFailure)
                    {
                        return StepResult.Failure(result.Error.Code, result.Error.Message);
                    }

                    ctx.Set(MonthlyMockExamWorkflowKeys.BandPredictionId, result.Value.BandPredictionId);
                    var output = JsonSerializer.Serialize(new
                    {
                        band_prediction_id = result.Value.BandPredictionId,
                        operation_id = result.Value.OperationId,
                        status = result.Value.Status,
                    });
                    return StepResult.Success(output);
                },
                MaxRetries: 0),

            // Step 3 (Conditional): Generate today's learning recommendation & daily plan,
            // but only if the Band Predictor step succeeded. Uses the existing
            // DailyLearningWorkflowRunner (Sprint 9) which handles the full AI pipeline.
            new StepDefinition(
                StepKey: "learning-recommendation",
                Kind: StepKind.Conditional,
                Condition: ctx => ctx.Has(MonthlyMockExamWorkflowKeys.BandPredictionId),
                Execute: async (ctx, ct) =>
                {
                    var today = DateOnly.FromDateTime(clock.UtcNow.DateTime);
                    // allowAiGeneration = true: the workflow has already passed entitlement/quota
                    // checks for the band-predictor step, so AI spend for recommendations is expected.
                    var planResult = await learningRunner
                        .RunForUserAsync(tenantId, userId, today, allowAiGeneration: true, ct)
                        .ConfigureAwait(false);

                    if (planResult.IsFailure)
                    {
                        // Non-fatal: a recommendation failure doesn't block the review.
                        ctx.Set(MonthlyMockExamWorkflowKeys.LearningRecommendationGenerated, false);
                        return StepResult.Failure(planResult.Error.Code, planResult.Error.Message);
                    }

                    ctx.Set(MonthlyMockExamWorkflowKeys.LearningRecommendationGenerated, true);
                    var output = JsonSerializer.Serialize(new { daily_plan_id = planResult.Value.Id.Value });
                    return StepResult.Success(output);
                },
                MaxRetries: 0),
        ]);
    }
}
