namespace IELTSLms.Abstractions.Jobs;

/// <summary>
/// Application-facing port for managing recurring (cron-based) background jobs.
/// Implemented by Hangfire (Backend Architecture §5.9).
/// </summary>
public interface IRecurringJobService
{
    /// <summary>Adds or updates a recurring job with the given cron expression.</summary>
    void AddOrUpdate<T>(string recurringJobId, System.Linq.Expressions.Expression<Func<T, Task>> methodCall, string cronExpression);

    /// <summary>Removes a recurring job.</summary>
    void RemoveIfExists(string recurringJobId);

    /// <summary>Triggers a recurring job for immediate execution.</summary>
    void TriggerJob(string recurringJobId);
}
