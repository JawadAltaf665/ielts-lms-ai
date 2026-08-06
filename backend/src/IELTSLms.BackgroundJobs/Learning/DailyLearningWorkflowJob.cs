using IELTSLms.Abstractions.Billing;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.Abstractions.Time;
using IELTSLms.Application.Learning.StudyPlanner;
using Microsoft.Extensions.Logging;

namespace IELTSLms.BackgroundJobs.Learning;

/// <summary>
/// The Daily Learning Workflow's recurring entry point (Sprint 9, Agentic AI Architecture §13.3):
/// Progress Analysis → Learning Recommendation → Study Planner, per tenant/active-learner cohort.
/// Entitlement-filtered per tenant (business rule: don't burn AI budget generating plans for tenants
/// without the feature) and quota-checked per learner — a learner whose tenant lacks the entitlement, or
/// who has exhausted the tenant's quota for the period, still gets a default-tasks plan via
/// <see cref="DailyLearningWorkflowRunner"/>'s <c>allowAiGeneration: false</c> path rather than nothing.
/// ponytail: iterates tenants/users sequentially in one job execution rather than fanning each user out
/// to its own Hangfire job; acceptable while tenant/cohort counts stay moderate — revisit (fan out via
/// <c>IBackgroundJobService.Enqueue</c> per user) if a nightly run starts running long.
/// </summary>
public sealed partial class DailyLearningWorkflowJob(
    ITenantRepository tenants, IProgressTrackingRepository progressTracking, IEntitlementService entitlements,
    IQuotaService quotas, DailyLearningWorkflowRunner runner, ITenantContextAccessor tenantContext, IClock clock,
    ILogger<DailyLearningWorkflowJob> logger)
{
    /// <summary>Mirrors <c>AiFeatures.DailyLearningWorkflowEntitlement</c> (Application, internal) — kept
    /// as a literal here rather than widening that class's visibility for two constants.</summary>
    private const string DailyLearningWorkflowEntitlement = "ai.daily-learning-workflow";

    /// <summary>Mirrors <c>AiFeatures.DailyLearningWorkflowQuota</c> (Application, internal).</summary>
    private const string DailyLearningWorkflowQuota = "ai.daily-learning-workflow.requests";

    private const int TenantPageSize = 100;

    /// <summary>A learner counts as "active" for the cohort sweep if they have recorded activity within
    /// this window.</summary>
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromDays(30);

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var planDate = DateOnly.FromDateTime(now.UtcDateTime);

        var tenantsProcessed = 0;
        var usersProcessed = 0;
        var skip = 0;

        while (true)
        {
            var page = await tenants.ListAsync(skip, TenantPageSize, cancellationToken).ConfigureAwait(false);
            if (page.Count == 0)
            {
                break;
            }

            skip += page.Count;

            foreach (var tenant in page)
            {
                var tenantId = tenant.Id.Value;
                var entitled = await entitlements
                    .HasFeatureAsync(tenantId, DailyLearningWorkflowEntitlement, cancellationToken)
                    .ConfigureAwait(false);
                if (!entitled)
                {
                    continue;
                }

                tenantContext.SetTenant(tenantId);
                tenantsProcessed++;

                var activeUserIds = await progressTracking
                    .ListActiveUserIdsAsync(tenantId, now - ActiveWindow, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var userId in activeUserIds)
                {
                    var allowAiGeneration = await TryConsumeQuotaAsync(tenantId, cancellationToken).ConfigureAwait(false);
                    var result = await runner
                        .RunForUserAsync(tenantId, userId, planDate, allowAiGeneration, cancellationToken)
                        .ConfigureAwait(false);

                    if (result.IsFailure)
                    {
                        UserRunFailed(logger, tenantId, userId, result.Error.Code);
                        continue;
                    }

                    usersProcessed++;
                }
            }
        }

        SweepCompleted(logger, tenantsProcessed, usersProcessed);
    }

    private async Task<bool> TryConsumeQuotaAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var quotaCheck = await quotas
            .CheckAsync(tenantId, DailyLearningWorkflowQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        if (!quotaCheck.Allowed)
        {
            return false;
        }

        var consumeResult = await quotas
            .ConsumeAsync(tenantId, DailyLearningWorkflowQuota, amount: 1, cancellationToken)
            .ConfigureAwait(false);
        return consumeResult.IsSuccess;
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Daily Learning Workflow run failed for tenant {TenantId} user {UserId} ({ErrorCode}).")]
    private static partial void UserRunFailed(ILogger logger, Guid tenantId, Guid userId, string errorCode);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Daily Learning Workflow sweep complete: {TenantsProcessed} entitled tenants, {UsersProcessed} learners.")]
    private static partial void SweepCompleted(ILogger logger, int tenantsProcessed, int usersProcessed);
}
