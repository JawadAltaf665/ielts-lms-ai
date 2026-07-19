using FluentValidation;
using FluentValidation.Results;
using IELTSLms.SharedKernel.Results;
using MediatR;

namespace IELTSLms.Application.Abstractions.Behaviors;

/// <summary>
/// Runs all FluentValidation validators registered for the incoming request before the handler.
/// On failure it short-circuits with a validation <see cref="Result"/> rather than throwing, so
/// callers get a structured error (Development Standards §10, API Design Standards §9).
/// </summary>
/// <typeparam name="TRequest">The request being handled.</typeparam>
/// <typeparam name="TResponse">The handler response; must be a <see cref="Result"/> or <c>Result&lt;T&gt;</c>.</typeparam>
public sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IValidator<TRequest>[] _validators = [.. validators];

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = new List<ValidationFailure>();
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
            {
                failures.AddRange(result.Errors);
            }
        }

        if (failures.Count == 0)
        {
            return await next().ConfigureAwait(false);
        }

        return CreateValidationResult(failures);
    }

    private static TResponse CreateValidationResult(List<ValidationFailure> failures)
    {
        // Surface the first failure as the primary error; the full set is preserved on the result.
        var first = failures[0];
        var error = Error.Validation(
            first.ErrorCode is { Length: > 0 } code ? code : "VALIDATION_ERROR",
            first.ErrorMessage);

        var details = failures
            .Select(f => new ValidationFailureDetail(
                f.PropertyName,
                f.ErrorCode is { Length: > 0 } fieldCode ? fieldCode : "VALIDATION_ERROR",
                f.ErrorMessage))
            .ToArray();

        var validationError = ValidationError.Create(error, details);

        // TResponse is either Result or Result<T>; build the matching failure instance.
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(validationError);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return (TResponse)failureMethod.Invoke(null, [validationError])!;
    }
}
