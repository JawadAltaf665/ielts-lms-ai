using IELTSLms.Abstractions.Context;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace IELTSLms.Infrastructure.Logging;

/// <summary>
/// Pushes UserId onto every log line for the request scope.
/// </summary>
internal sealed class UserIdLogEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userId = httpContextAccessor.HttpContext?.RequestServices
            .GetService(typeof(ICurrentUserService)) as ICurrentUserService;

        if (userId?.UserId is { } id)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", id));
        }
    }
}
