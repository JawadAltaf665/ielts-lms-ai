using IELTSLms.Domain.AI;
using IELTSLms.Domain.Learning.Events;
using IELTSLms.SharedKernel.Domain;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Domain.Learning;

/// <summary>
/// Adaptive per-day study agenda for a learner (ERD ENT-034; Agentic AI Architecture §13.3 Daily
/// Learning Workflow, Study Planner Agent step). This sprint is the first real writer — the entity
/// shape follows the ERD's existing catalog entry unchanged: <see cref="LearningPathId"/> is a soft,
/// nullable cross-aggregate reference (identifier only, per ERD §6 relationship rules) to a
/// <c>LearningPath</c> that no sprint has implemented yet, so it is always <see langword="null"/> for
/// now — tracked as a documented gap, not silently built out here (out of this sprint's scope).
/// <see cref="PlanItemsJson"/> holds the generated task list (see <c>DailyPlanItemDto</c>, Application)
/// with duration and per-item completion state, matching the ERD description exactly.
/// </summary>
public sealed class DailyPlan : TenantAuditableEntity<DailyPlanId>
{
    private DailyPlan(
        DailyPlanId id, Guid tenantId, Guid userId, DateOnly planDate, Guid? learningPathId, string planItemsJson,
        AiResponseId? aiResponseId, DateTimeOffset nowUtc)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        PlanDate = planDate;
        LearningPathId = learningPathId;
        PlanItemsJson = planItemsJson;
        AiResponseId = aiResponseId;
        GeneratedAtUtc = nowUtc;
        CreatedAtUtc = nowUtc;
    }

    private DailyPlan()
    {
        PlanItemsJson = null!;
    }

    public Guid UserId { get; private init; }

    public DateOnly PlanDate { get; private init; }

    /// <summary>Soft, nullable cross-aggregate reference to a <c>LearningPath</c> (ERD ENT-012) —
    /// always <see langword="null"/> until a future sprint implements that entity (see type doc comment).</summary>
    public Guid? LearningPathId { get; private init; }

    /// <summary>Structured JSON: a list of <c>{ id, skill, title, description, durationMinutes,
    /// isCompleted, completedAtUtc }</c> — see <c>DailyPlanItemDto</c> (Application).</summary>
    public string PlanItemsJson { get; private set; }

    public AiResponseId? AiResponseId { get; private init; }

    public DateTimeOffset GeneratedAtUtc { get; private init; }

    public static Result<DailyPlan> Create(
        DailyPlanId id, Guid tenantId, Guid userId, DateOnly planDate, Guid? learningPathId, string planItemsJson,
        AiResponseId? aiResponseId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(planItemsJson))
        {
            return Result.Failure<DailyPlan>(DailyPlanDomainErrors.ItemsRequired);
        }

        var plan = new DailyPlan(id, tenantId, userId, planDate, learningPathId, planItemsJson, aiResponseId, nowUtc);
        plan.RaiseDomainEvent(new DailyPlanGeneratedDomainEvent(tenantId, userId, id.Value, planDate));
        return Result.Success(plan);
    }

    /// <summary>Replaces the item list — used after marking one item complete (Application composes the
    /// updated JSON; this entity stays opaque to the item shape, same convention as <see cref="WeaknessProfile.Recompute"/>).</summary>
    public Result UpdateItems(string planItemsJson, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(planItemsJson))
        {
            return Result.Failure(DailyPlanDomainErrors.ItemsRequired);
        }

        PlanItemsJson = planItemsJson;
        UpdatedAtUtc = nowUtc;
        return Result.Success();
    }
}

internal static class DailyPlanDomainErrors
{
    public static readonly Error ItemsRequired = Error.Validation(
        "VALIDATION_DAILY_PLAN_ITEMS_REQUIRED", "Daily plan items are required.");
}
