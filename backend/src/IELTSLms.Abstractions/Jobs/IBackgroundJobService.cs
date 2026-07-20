using System.Linq.Expressions;

namespace IELTSLms.Abstractions.Jobs;

/// <summary>
/// Application-facing port for enqueuing fire-and-forget background jobs.
/// Implemented by Hangfire in the BackgroundJobs layer (Backend Architecture §5.9).
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>Enqueues a job for immediate execution.</summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>Enqueues a job for immediate execution on a specific queue.</summary>
    string Enqueue(string queue, Expression<Func<Task>> methodCall);

    /// <summary>Enqueues a job for immediate execution (service method).</summary>
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>Schedules a job for delayed execution.</summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>Schedules a job for delayed execution (service method).</summary>
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    /// <summary>Schedules a continuation job that runs after the parent completes.</summary>
    string ContinueWith(string parentJobId, Expression<Func<Task>> methodCall);

    /// <summary>Deletes a pending or scheduled job.</summary>
    bool Delete(string jobId);

    /// <summary>Re-enqueues a failed or deleted job.</summary>
    bool Requeue(string jobId);
}
