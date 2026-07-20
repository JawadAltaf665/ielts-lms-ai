using System.Linq.Expressions;
using Hangfire;
using IELTSLms.Abstractions.Jobs;

namespace IELTSLms.BackgroundJobs.Services;

/// <summary>
/// Hangfire-backed implementation of <see cref="IBackgroundJobService"/>.
/// Wraps <see cref="IBackgroundJobClient"/> to keep application code decoupled from Hangfire.
/// </summary>
internal sealed class HangfireBackgroundJobService(IBackgroundJobClient client) : IBackgroundJobService
{
    public string Enqueue(Expression<Func<Task>> methodCall) =>
        client.Enqueue(methodCall);

    public string Enqueue(string queue, Expression<Func<Task>> methodCall) =>
        client.Create(methodCall, new Hangfire.States.EnqueuedState(queue));

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) =>
        client.Enqueue(methodCall);

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) =>
        client.Schedule(methodCall, delay);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) =>
        client.Schedule(methodCall, delay);

    public string ContinueWith(string parentJobId, Expression<Func<Task>> methodCall) =>
        client.ContinueJobWith(parentJobId, methodCall);

    public bool Delete(string jobId) =>
        client.Delete(jobId);

    public bool Requeue(string jobId) =>
        client.Requeue(jobId);
}
