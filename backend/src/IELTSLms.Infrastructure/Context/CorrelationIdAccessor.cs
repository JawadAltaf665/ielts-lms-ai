using IELTSLms.Abstractions.Context;
using Microsoft.AspNetCore.Http;

namespace IELTSLms.Infrastructure.Context;

internal sealed class CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor) : ICorrelationIdProvider
{
    private const string CorrelationIdKey = "X-Correlation-Id";

    public string CorrelationId =>
        httpContextAccessor.HttpContext?.Items[CorrelationIdKey]?.ToString()
        ?? Guid.CreateVersion7().ToString("n");
}
