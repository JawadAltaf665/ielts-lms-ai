using Hangfire;
using IELTSLms.Abstractions.Jobs;

namespace IELTSLms.BackgroundJobs.Services;

/// <summary>
/// Hangfire-backed implementation of <see cref="IRecurringJobService"/>. Uses the DI-resolved
/// <see cref="IRecurringJobManager"/> rather than the static <see cref="RecurringJob"/> bridge —
/// the static API needs the legacy <c>JobStorage.Current</c> to have been set as a side effect of
/// startup, which newer Hangfire versions no longer guarantee (its own exception says as much:
/// "ensure service-based APIs are used instead of static ones, like IRecurringJobManager instead of
/// RecurringJob"). The instance API works from the moment <c>AddHangfire</c> registers it.
/// </summary>
internal sealed class HangfireRecurringJobService(IRecurringJobManager recurringJobManager) : IRecurringJobService
{
    public void AddOrUpdate<T>(
        string recurringJobId,
        System.Linq.Expressions.Expression<Func<T, Task>> methodCall,
        string cronExpression)
    {
        recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression);
    }

    public void RemoveIfExists(string recurringJobId)
    {
        recurringJobManager.RemoveIfExists(recurringJobId);
    }

    public void TriggerJob(string recurringJobId)
    {
        recurringJobManager.Trigger(recurringJobId);
    }
}
