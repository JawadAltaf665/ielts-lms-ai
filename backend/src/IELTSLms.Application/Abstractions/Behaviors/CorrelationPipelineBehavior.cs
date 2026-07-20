using IELTSLms.Abstractions.Context;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Ensures every log entry emitted within a handler scope carries the correlation id.
/// Pushes the id into the logger scope so downstream structured logging picks it up automatically.
/// </summary>
public sealed class CorrelationPipelineBehavior<TRequest, TResponse>(
    ICorrelationIdProvider correlationIdProvider,
    ILogger<CorrelationPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationIdProvider.CorrelationId,
        }))
        {
            return await next().ConfigureAwait(false);
        }
    }
}
