using IELTSLms.Api.Constants;
using IELTSLms.SharedKernel.Results;
using Microsoft.AspNetCore.Http;

namespace IELTSLms.Api.Extensions;

/// <summary>
/// Translates a failed <see cref="Result"/>/<see cref="Error"/> into an RFC 7807 problem response.
/// Centralizing this keeps every endpoint's error shape identical (API Design Standards §9) and
/// maps <see cref="ErrorType"/> to the correct HTTP status without endpoints touching HTTP codes.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Builds a problem-details <see cref="IResult"/> for a failed result.</summary>
    /// <exception cref="InvalidOperationException">Thrown if the result is successful.</exception>
    public static IResult ToProblem(this Result result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be converted to a problem response.");
        }

        return result.Error.ToProblem(httpContext);
    }

    /// <summary>Builds a problem-details <see cref="IResult"/> directly from an <see cref="Error"/>.</summary>
    public static IResult ToProblem(this Error error, HttpContext httpContext)
    {
        var statusCode = ToStatusCode(error.Type);
        var (type, title) = ProblemTypes.ForErrorType(error.Type);

        var extensions = new Dictionary<string, object?>
        {
            ["code"] = error.Code,
        };

        if (httpContext.Items.TryGetValue(HeaderNames.CorrelationId, out var correlationId) && correlationId is not null)
        {
            extensions["correlationId"] = correlationId.ToString();
        }

        if (error is ValidationError validationError)
        {
            extensions["errors"] = validationError.Failures
                .Select(f => new
                {
                    field = f.PropertyName,
                    code = f.Code,
                    message = f.Message,
                })
                .ToArray();
        }

        return Results.Problem(
            detail: error.Message,
            instance: httpContext.Request.Path,
            statusCode: statusCode,
            title: title,
            type: type,
            extensions: extensions);
    }

    private static int ToStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError,
    };
}
