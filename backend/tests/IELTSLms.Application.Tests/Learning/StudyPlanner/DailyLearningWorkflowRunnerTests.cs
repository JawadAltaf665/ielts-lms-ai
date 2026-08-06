using IELTSLms.Application.Exceptions;
using IELTSLms.Application.Learning.StudyPlanner;
using IELTSLms.Application.Tests.AI.Fakes;
using IELTSLms.Application.Tests.Learning.Fakes;
using IELTSLms.Domain.Learning;
using Microsoft.Extensions.Logging.Abstractions;

namespace IELTSLms.Application.Tests.Learning.StudyPlanner;

public sealed class DailyLearningWorkflowRunnerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly PlanDate = DateOnly.FromDateTime(Now.UtcDateTime);

    private static DailyLearningWorkflowRunner CreateRunner(
        FakeProgressTrackingRepository progressTracking, FakeWeaknessProfileRepository weaknessProfiles,
        FakeLearningRecommendationRepository recommendations, FakeDailyPlanRepository dailyPlans, FakeAiGateway gateway)
    {
        var providers = new FakeAiProviderRepository();
        var prompts = new FakeAiPromptRepository();
        var progressAnalysis = new IELTSLms.Application.Learning.WeaknessDetection.ProgressAnalysisService(
            progressTracking, weaknessProfiles, providers, prompts, new FakeAiResponseRepository(),
            new IELTSLms.Application.Tests.Writing.Fakes.FakeWritingSubmissionRepository(),
            new IELTSLms.Application.Tests.Writing.Fakes.FakeWritingEvaluationRepository(),
            new IELTSLms.Application.Tests.Speaking.Fakes.FakeSpeakingAttemptRepository(),
            new IELTSLms.Application.Tests.Speaking.Fakes.FakeSpeakingEvaluationRepository(),
            new IELTSLms.Application.Tests.Reading.Fakes.FakeReadingAttemptRepository(),
            new IELTSLms.Application.Tests.Listening.Fakes.FakeListeningAttemptRepository(),
            new IELTSLms.Application.Tests.Grammar.Fakes.FakeGrammarAttemptRepository(),
            new IELTSLms.Application.Tests.Vocabulary.Fakes.FakeVocabularyProgressRepository(),
            gateway, new FakeGuidProvider());

        return new DailyLearningWorkflowRunner(
            progressAnalysis, recommendations, dailyPlans, providers, prompts, new FakeAiResponseRepository(), gateway,
            new FakeUnitOfWork(), new FakeClock(Now), new FakeGuidProvider(), NullLogger<DailyLearningWorkflowRunner>.Instance);
    }

    [Fact]
    public async Task An_existing_plan_for_today_is_returned_as_is_without_regenerating()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existing = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), tenantId, userId, PlanDate, null,
            """[{"id":"11111111-1111-1111-1111-111111111111","skill":"Grammar","title":"t","description":"d","durationMinutes":15,"isCompleted":false,"completedAtUtc":null}]""",
            null, Now).Value;
        dailyPlans.Seed(existing);
        var gateway = new FakeAiGateway();

        var runner = CreateRunner(
            new FakeProgressTrackingRepository(), new FakeWeaknessProfileRepository(), new FakeLearningRecommendationRepository(),
            dailyPlans, gateway);

        var result = await runner.RunForUserAsync(tenantId, userId, PlanDate, allowAiGeneration: true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value.Id);
        Assert.Equal(0, gateway.InvokeCallCount);
        Assert.Single(dailyPlans.Plans);
    }

    [Fact]
    public async Task Ai_generation_disallowed_falls_back_to_default_tasks_without_calling_the_gateway()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        var gateway = new FakeAiGateway();

        var runner = CreateRunner(
            new FakeProgressTrackingRepository(), new FakeWeaknessProfileRepository(), new FakeLearningRecommendationRepository(),
            dailyPlans, gateway);

        var result = await runner.RunForUserAsync(
            Guid.NewGuid(), Guid.NewGuid(), PlanDate, allowAiGeneration: false, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.PlanItemsJson);
        Assert.Equal(0, gateway.InvokeCallCount);
    }

    [Fact]
    public async Task Never_returns_empty_for_a_brand_new_learner_below_the_weakness_data_threshold()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        // No ProgressTracking rows seeded at all — a genuinely brand-new learner.
        var runner = CreateRunner(
            new FakeProgressTrackingRepository(), new FakeWeaknessProfileRepository(), new FakeLearningRecommendationRepository(),
            dailyPlans, new FakeAiGateway());

        var result = await runner.RunForUserAsync(
            Guid.NewGuid(), Guid.NewGuid(), PlanDate, allowAiGeneration: true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.PlanItemsJson);
    }

    [Fact]
    public async Task Losing_the_unique_index_race_against_a_concurrent_generation_returns_the_winners_plan_not_a_duplicate()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var winner = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), tenantId, userId, PlanDate, null,
            """[{"id":"22222222-2222-2222-2222-222222222222","skill":"Reading","title":"t","description":"d","durationMinutes":20,"isCompleted":false,"completedAtUtc":null}]""",
            null, Now).Value;
        dailyPlans.ThrowOnNextAdd = new UniqueConstraintViolationException("daily_plans", "ix_daily_plans_tenant_user_date_unique");
        dailyPlans.WinnerToInsertOnRace = winner;

        var runner = CreateRunner(
            new FakeProgressTrackingRepository(), new FakeWeaknessProfileRepository(), new FakeLearningRecommendationRepository(),
            dailyPlans, new FakeAiGateway());

        var result = await runner.RunForUserAsync(tenantId, userId, PlanDate, allowAiGeneration: false, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(winner.Id, result.Value.Id);
        // Exactly one row survives — no duplicate (UserId, PlanDate) plan.
        Assert.Single(dailyPlans.Plans);
    }
}
