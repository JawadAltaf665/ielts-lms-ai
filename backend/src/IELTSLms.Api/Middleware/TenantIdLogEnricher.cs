using IELTSLms.Abstractions.Tenancy;
using Serilog.Core;
using Serilog.Events;

namespace IELTSLms.Api.Middleware;

/// <summary>
/// Serilog enricher that stamps every log event with the ambient <c>TenantId</c> property when a
/// tenant has been resolved for the current request. This lets structured-log backends (Seq, ELK,
/// Datadog) filter and aggregate by tenant without per-handler log boilerplate.
/// </summary>
internal sealed class TenantIdLogEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    private const string PropertyName = "TenantId";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var tenantContext = httpContextAccessor.HttpContext?.RequestServices?.GetService<ITenantContext>();
        if (tenantContext?.TenantId is { } tenantId)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(PropertyName, tenantId));
        }
    }
}
