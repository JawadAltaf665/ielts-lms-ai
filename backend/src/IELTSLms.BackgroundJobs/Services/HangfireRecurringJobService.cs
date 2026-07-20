using Hangfire;
using IELTSLms.Abstractions.Jobs;

namespace IELTSLms.BackgroundJobs.Services;

/// <summary>
/// Hangfire-backed implementation of <see cref="IRecurringJobService"/>.
/// Wraps the Hangfire <see cref="RecurringJob"/> static API to keep application code decoupled.
/// </summary>
internal sealed class HangfireRecurringJobService : IRecurringJobService
{
    public void AddOrUpdate<T>(
        string recurringJobId,
        System.Linq.Expressions.Expression<Func<T, Task>> methodCall,
        string cronExpression)
    {
        RecurringJob.AddOrUpdate(recurringJobId, methodCall, cronExpression);
    }

    public void RemoveIfExists(string recurringJobId)
    {
        RecurringJob.RemoveIfExists(recurringJobId);
    }

    public void TriggerJob(string recurringJobId)
    {
        RecurringJob.TriggerJob(recurringJobId);
    }
}
