using IELTSLms.Application.Learning.StudyPlanner.Commands.CompleteDailyPlanItem;
using IELTSLms.Application.Tests.Learning.Fakes;
using IELTSLms.Domain.Learning;

namespace IELTSLms.Application.Tests.Learning.StudyPlanner;

public sealed class CompleteDailyPlanItemCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly PlanDate = DateOnly.FromDateTime(Now.UtcDateTime);
    private static readonly Guid ItemId = Guid.NewGuid();

    private static string OneItem(bool isCompleted = false) =>
        $$"""[{"id":"{{ItemId}}","skill":"Grammar","title":"Do a Grammar exercise","description":"desc","durationMinutes":15,"isCompleted":{{isCompleted.ToString().ToLowerInvariant()}},"completedAtUtc":null}]""";

    [Fact]
    public async Task Completing_an_open_item_marks_it_complete_and_records_progress()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var plan = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), tenantId, userId, PlanDate, null, OneItem(), null, Now).Value;
        dailyPlans.Seed(plan);
        var progressTrackingService = new FakeProgressTrackingService();
        var unitOfWork = new FakeUnitOfWork();

        var handler = new CompleteDailyPlanItemCommandHandler(
            dailyPlans, progressTrackingService, unitOfWork, new FakeClock(Now.AddHours(1)));

        var result = await handler.Handle(new CompleteDailyPlanItemCommand(tenantId, userId, ItemId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.True(item.IsCompleted);
        Assert.Equal(Now.AddHours(1), item.CompletedAtUtc);
        var recorded = Assert.Single(progressTrackingService.RecordedActivities);
        Assert.Equal(LearningSkill.Grammar, recorded.Skill);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Completing_an_already_completed_item_fails_without_double_recording_progress()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var plan = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), tenantId, userId, PlanDate, null, OneItem(isCompleted: true), null, Now).Value;
        dailyPlans.Seed(plan);
        var progressTrackingService = new FakeProgressTrackingService();

        var handler = new CompleteDailyPlanItemCommandHandler(
            dailyPlans, progressTrackingService, new FakeUnitOfWork(), new FakeClock(Now.AddHours(1)));

        var result = await handler.Handle(new CompleteDailyPlanItemCommand(tenantId, userId, ItemId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("CONFLICT_DAILY_PLAN_ITEM_ALREADY_COMPLETED", result.Error.Code);
        Assert.Empty(progressTrackingService.RecordedActivities);
    }

    [Fact]
    public async Task No_plan_for_today_fails()
    {
        var handler = new CompleteDailyPlanItemCommandHandler(
            new FakeDailyPlanRepository(), new FakeProgressTrackingService(), new FakeUnitOfWork(), new FakeClock(Now));

        var result = await handler.Handle(
            new CompleteDailyPlanItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_DAILY_PLAN", result.Error.Code);
    }

    [Fact]
    public async Task Unknown_item_id_fails()
    {
        var dailyPlans = new FakeDailyPlanRepository();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var plan = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), tenantId, userId, PlanDate, null, OneItem(), null, Now).Value;
        dailyPlans.Seed(plan);

        var handler = new CompleteDailyPlanItemCommandHandler(
            dailyPlans, new FakeProgressTrackingService(), new FakeUnitOfWork(), new FakeClock(Now.AddHours(1)));

        var result = await handler.Handle(
            new CompleteDailyPlanItemCommand(tenantId, userId, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("NOT_FOUND_DAILY_PLAN_ITEM", result.Error.Code);
    }
}
