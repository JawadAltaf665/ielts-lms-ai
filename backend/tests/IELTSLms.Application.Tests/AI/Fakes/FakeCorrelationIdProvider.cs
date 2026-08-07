using IELTSLms.Abstractions.Context;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeCorrelationIdProvider : ICorrelationIdProvider
{
    public string CorrelationId { get; init; } = "corr_test";
}
