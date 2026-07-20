using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Checks the cancellation token early in the pipeline to abort before doing any work
/// if the client has already disconnected.
/// </summary>
public sealed class CancellationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await next().ConfigureAwait(false);
    }
}
