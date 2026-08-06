using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning;
using IELTSLms.Domain.Learning.Events;

namespace IELTSLms.Domain.Tests.Learning;

public sealed class DailyPlanTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly PlanDate = DateOnly.FromDateTime(Now.UtcDateTime);

    private const string OneItem = """[{"id":"11111111-1111-1111-1111-111111111111","skill":"Grammar","title":"Do a Grammar exercise","description":"desc","durationMinutes":15,"isCompleted":false,"completedAtUtc":null}]""";

    [Fact]
    public void Create_with_items_succeeds_and_raises_daily_plan_generated()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), tenantId, userId, PlanDate, learningPathId: null, OneItem, aiResponseId: null, Now);

        Assert.True(result.IsSuccess);
        var plan = result.Value;
        var raised = Assert.Single(plan.DomainEvents);
        var generated = Assert.IsType<DailyPlanGeneratedDomainEvent>(raised);
        Assert.Equal(tenantId, generated.TenantId);
        Assert.Equal(userId, generated.UserId);
        Assert.Equal(plan.Id.Value, generated.DailyPlanId);
        Assert.Equal(PlanDate, generated.PlanDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_without_items_fails(string? itemsJson)
    {
        var result = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), PlanDate, null, itemsJson!, null, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VALIDATION_DAILY_PLAN_ITEMS_REQUIRED", result.Error.Code);
    }

    [Fact]
    public void UpdateItems_replaces_the_item_list()
    {
        var plan = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), PlanDate, null, OneItem, null, Now).Value;

        var updated = plan.UpdateItems(OneItem.Replace("\"isCompleted\":false", "\"isCompleted\":true"), Now.AddHours(1));

        Assert.True(updated.IsSuccess);
        Assert.Contains("\"isCompleted\":true", plan.PlanItemsJson);
    }

    [Fact]
    public void LearningPathId_stays_null_until_a_future_sprint_implements_that_entity()
    {
        var plan = DailyPlan.Create(
            DailyPlanId.From(Guid.NewGuid()), Guid.NewGuid(), Guid.NewGuid(), PlanDate, null, OneItem, null, Now).Value;

        Assert.Null(plan.LearningPathId);
    }
}
