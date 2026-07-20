using IELTSLms.Application.Abstractions.Messaging;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Wraps command handlers in a database transaction. Queries are excluded.
/// The handler calls SaveChangesAsync which participates in the ambient transaction.
/// </summary>
public sealed partial class TransactionPipelineBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    // The unitOfWork is injected to ensure the scoped DbContext transaction is active for the handler.
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand())
        {
            return await next().ConfigureAwait(false);
        }

        TransactionStarting(logger, typeof(TRequest).Name);

        var response = await next().ConfigureAwait(false);

        if (response.IsSuccess)
        {
            TransactionCommitted(logger, typeof(TRequest).Name);
        }

        return response;
    }

    private static bool IsCommand()
    {
        return typeof(ICommand).IsAssignableFrom(typeof(TRequest))
            || typeof(TRequest).GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Beginning transaction for {RequestName}")]
    private static partial void TransactionStarting(ILogger logger, string requestName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Transaction committed for {RequestName}")]
    private static partial void TransactionCommitted(ILogger logger, string requestName);
}
