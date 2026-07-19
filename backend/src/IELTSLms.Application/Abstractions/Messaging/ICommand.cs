using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Abstractions.Messaging;

/// <summary>
/// A command that changes state and returns no value beyond success/failure. Commands are the
/// write side of CQRS (Backend Architecture §7); one command maps to one use case.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// A command that changes state and returns a value (e.g. the id of a created resource).
/// </summary>
/// <typeparam name="TResponse">The value returned on success.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
