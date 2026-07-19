using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Abstractions.Messaging;

/// <summary>Handles an <see cref="IQuery{TResponse}"/>.</summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The data returned on success.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
