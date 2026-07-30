using IELTSLms.Abstractions.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAiGateway : IAiGateway
{
    public int InvokeCallCount { get; private set; }

    public Result<AiGatewayInvocationResult>? NextResult { get; set; }

    public Task<Result<AiGatewayInvocationResult>> InvokeAsync(AiGatewayRequest request, CancellationToken cancellationToken = default)
    {
        InvokeCallCount++;
        return Task.FromResult(NextResult ?? Result.Success(new AiGatewayInvocationResult(
            """{"corrections":[]}""", "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false)));
    }
}
