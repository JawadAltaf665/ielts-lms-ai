using System.Text.Json;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Learning.StudyPlanner.Queries.GetDailyPlan;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.StudyPlanner.Commands.CompleteDailyPlanItem;

/// <summary>Marks one item on today's plan complete. Business rule: feeds back into
/// <c>ProgressTracking</c> through the existing shared service — no parallel progress-write path.</summary>
public sealed class CompleteDailyPlanItemCommandHandler(
    IDailyPlanRepository dailyPlans, IProgressTrackingService progressTrackingService, IUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CompleteDailyPlanItemCommand, DailyPlanResponse>
{
    public async Task<Result<DailyPlanResponse>> Handle(CompleteDailyPlanItemCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var planDate = DateOnly.FromDateTime(now.UtcDateTime);

        var plan = await dailyPlans.GetByUserAndDateAsync(request.TenantId, request.UserId, planDate, cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            return Result.Failure<DailyPlanResponse>(LearningErrors.DailyPlanNotFound);
        }

        var items = JsonSerializer.Deserialize<List<DailyPlanItemDto>>(plan.PlanItemsJson, LearningJsonOptions.Instance) ?? [];
        var index = items.FindIndex(i => i.Id == request.ItemId);
        if (index < 0)
        {
            return Result.Failure<DailyPlanResponse>(LearningErrors.DailyPlanItemNotFound);
        }

        var item = items[index];
        if (item.IsCompleted)
        {
            return Result.Failure<DailyPlanResponse>(LearningErrors.DailyPlanItemAlreadyCompleted);
        }

        items[index] = item with { IsCompleted = true, CompletedAtUtc = now };

        var updated = plan.UpdateItems(JsonSerializer.Serialize(items, LearningJsonOptions.Instance), now);
        if (updated.IsFailure)
        {
            return Result.Failure<DailyPlanResponse>(updated.Error);
        }

        await progressTrackingService
            .RecordSkillActivityAsync(request.TenantId, request.UserId, item.Skill, now, cancellationToken)
            .ConfigureAwait(false);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success(new DailyPlanResponse(plan.Id.Value, plan.PlanDate, items));
    }
}
