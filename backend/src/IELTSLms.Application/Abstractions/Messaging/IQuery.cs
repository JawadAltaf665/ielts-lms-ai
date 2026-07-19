using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Abstractions.Messaging;

/// <summary>
/// A read-only request that returns data and never mutates state. Queries are the read side of
/// CQRS (Backend Architecture §7) and may bypass the domain model for efficient projections.
/// </summary>
/// <typeparam name="TResponse">The data returned on success.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
