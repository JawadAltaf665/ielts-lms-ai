using System.Text.Json;
using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.AI;
using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Application.Learning.StudyPlanner;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Learning.StudyPlanner.Queries.GetDailyPlan;

/// <summary>
/// Gets today's plan, generating it on the first request of the day if the scheduled sweep hasn't run
/// yet (business rule: "never returns empty for an active learner"). Deliberately not a pure read — the
/// lazy-fallback generation is a documented exception to the usual query/no-side-effects split, same
/// spirit as the sprint's own "lazy fallback" wording. Entitlement/quota is checked here (the query
/// handler, not <c>DailyLearningWorkflowRunner</c>) so a not-entitled/exhausted tenant still gets a
/// default-tasks plan instead of an AI call.
/// </summary>
public sealed class GetDailyPlanQueryHandler(
    DailyLearningWorkflowRunner runner, IEntitlementService entitlements, IQuotaService quotas, IClock clock)
    : IQueryHandler<GetDailyPlanQuery, DailyPlanResponse>
{
    public async Task<Result<DailyPlanResponse>> Handle(GetDailyPlanQuery request, CancellationToken cancellationToken)
    {
        var planDate = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var allowAiGeneration = await CanAffordAiGenerationAsync(request.TenantId, cancellationToken).ConfigureAwait(false);

        var result = await runner
            .RunForUserAsync(request.TenantId, request.UserId, planDate, allowAiGeneration, cancellationToken)
            .ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result.Failure<DailyPlanResponse>(result.Error);
        }

        var plan = result.Value;
        var items = JsonSerializer.Deserialize<List<DailyPlanItemDto>>(plan.PlanItemsJson, LearningJsonOptions.Instance) ?? [];
        return Result.Success(new DailyPlanResponse(plan.Id.Value, plan.PlanDate, items));
    }

    private async Task<bool> CanAffordAiGenerationAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var entitled = await entitlements
            .HasFeatureAsync(tenantId, AiFeatures.DailyLearningWorkflowEntitlement, cancellationToken)
            .ConfigureAwait(false);
        if (!entitled)
        {
            return false;
        }

        var quotaCheck = await quotas
            .CheckAsync(tenantId, AiFeatures.DailyLearningWorkflowQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return false;
        }

        var consumeResult = await quotas
            .ConsumeAsync(tenantId, AiFeatures.DailyLearningWorkflowQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        return consumeResult.IsSuccess;
    }
}
