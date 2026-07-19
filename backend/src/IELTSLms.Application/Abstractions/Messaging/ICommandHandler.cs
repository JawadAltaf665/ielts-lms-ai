using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Abstractions.Messaging;

/// <summary>Handles an <see cref="ICommand"/>.</summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>Handles an <see cref="ICommand{TResponse}"/>.</summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The value returned on success.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
