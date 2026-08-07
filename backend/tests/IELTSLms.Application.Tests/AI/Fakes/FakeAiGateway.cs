using IELTSLms.Abstractions.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiGateway : IAiGateway
{
    public int InvokeCallCount { get; private set; }

    public Result<AiGatewayInvocationResult>? NextResult { get; set; }

    /// <summary>Consumed one-per-call, in order, before falling back to <see cref="NextResult"/> — for
    /// tests that need a different result per invocation (e.g. one per calibration sample).</summary>
    public Queue<Result<AiGatewayInvocationResult>> ResultQueue { get; } = new();

    public Task<Result<AiGatewayInvocationResult>> InvokeAsync(AiGatewayRequest request, CancellationToken cancellationToken = default)
    {
        InvokeCallCount++;
        if (ResultQueue.Count > 0)
        {
            return Task.FromResult(ResultQueue.Dequeue());
        }

        return Task.FromResult(NextResult ?? Result.Success(new AiGatewayInvocationResult(
            """{"corrections":[]}""", "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false)));
    }
}
