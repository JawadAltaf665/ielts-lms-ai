using System.Linq.Expressions;
using IELTSLms.Abstractions.Jobs;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeBackgroundJobService : IBackgroundJobService
{
    public int EnqueueCallCount { get; private set; }

    public string Enqueue(Expression<Func<Task>> methodCall) => Record();

    public string Enqueue(string queue, Expression<Func<Task>> methodCall) => Record();

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => Record();

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay) => Record();

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) => Record();

    public string ContinueWith(string parentJobId, Expression<Func<Task>> methodCall) => Record();

    public bool Delete(string jobId) => true;

    public bool Requeue(string jobId) => true;

    private string Record()
    {
        EnqueueCallCount++;
        return Guid.NewGuid().ToString();
    }
}
