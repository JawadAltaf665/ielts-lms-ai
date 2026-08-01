using IELTSLms.Abstractions.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.Tests.Writing.Fakes;

/// <summary>
/// Unlike <c>FakeAiGateway</c> (AI.Fakes — one canned result for every call), the Writing Evaluation
/// Workflow makes three distinct calls (grammar-coach, vocabulary-analysis, writing-examiner) in the
/// same job run, so this fake dispatches a configurable result per <see cref="AiGatewayRequest.Feature"/>.
/// </summary>
internal sealed class FakeWritingAiGateway : IAiGateway
{
    private readonly Dictionary<string, Result<AiGatewayInvocationResult>> _resultsByFeature = [];

    public int InvokeCallCount { get; private set; }

    public void SetResult(string feature, Result<AiGatewayInvocationResult> result) => _resultsByFeature[feature] = result;

    public Task<Result<AiGatewayInvocationResult>> InvokeAsync(AiGatewayRequest request, CancellationToken cancellationToken = default)
    {
        InvokeCallCount++;
        if (_resultsByFeature.TryGetValue(request.Feature, out var result))
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(Result.Success(new AiGatewayInvocationResult(
            "{}", "claude-3-5-haiku", 10, 5, 100, 0, "USD", "msg_1", FromCache: false)));
    }
}
