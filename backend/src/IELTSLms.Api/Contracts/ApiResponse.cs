using IELTSLms.Api.Constants;

namespace IELTSLms.Api.Contracts;

/// <summary>
/// The standard success envelope every endpoint returns so clients parse uniformly (API Design
/// Standards §8.1): the payload under <c>data</c> with optional <c>meta</c>. Errors never use this
/// shape — they use RFC 7807 problem documents (§9).
/// </summary>
/// <typeparam name="T">The payload type carried under <c>data</c>.</typeparam>
/// <param name="Data">The resource or collection returned.</param>
/// <param name="Meta">Optional envelope metadata (request/correlation id, timestamp).</param>
public sealed record ApiResponse<T>(T Data, ApiResponseMeta? Meta = null);

/// <summary>Optional envelope metadata (API Design Standards §8.1).</summary>
/// <param name="RequestId">Server-generated id for this request, if available.</param>
/// <param name="CorrelationId">Correlation id linking related requests, if available.</param>
/// <param name="Timestamp">When the response was produced (UTC, ISO 8601).</param>
public sealed record ApiResponseMeta(string? RequestId, string? CorrelationId, DateTimeOffset Timestamp);

/// <summary>An empty success payload for operations that return no resource (uniform envelope shape).</summary>
public sealed record EmptyPayload;

/// <summary>Factory helpers that wrap a payload in the standard envelope with populated metadata.</summary>
public static class ApiResponse
{
    /// <summary>Wraps <paramref name="data"/> in the envelope, stamping request/correlation ids and a timestamp.</summary>
    public static ApiResponse<T> From<T>(T data, HttpContext httpContext) =>
        new(data, BuildMeta(httpContext));

    /// <summary>Builds an envelope with an empty payload for no-content-style successes that still return a body.</summary>
    public static ApiResponse<EmptyPayload> Empty(HttpContext httpContext) =>
        new(new EmptyPayload(), BuildMeta(httpContext));

    private static ApiResponseMeta BuildMeta(HttpContext httpContext)
    {
        var correlationId = httpContext.Items.TryGetValue(HeaderNames.CorrelationId, out var value)
            ? value?.ToString()
            : null;

        return new ApiResponseMeta(httpContext.TraceIdentifier, correlationId, DateTimeOffset.UtcNow);
    }
}
