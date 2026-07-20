using IELTSLms.Abstractions.Context;
using IELTSLms.Abstractions.Tenancy;
using Microsoft.AspNetCore.Http;

namespace IELTSLms.Infrastructure.Context;

internal sealed class RequestContextAccessor(
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserService currentUserService,
    ITenantContext tenantContext) : IRequestContext
{
    private const string CorrelationIdKey = "X-Correlation-Id";

    public string CorrelationId =>
        httpContextAccessor.HttpContext?.Items[CorrelationIdKey]?.ToString()
        ?? Guid.CreateVersion7().ToString("n");

    public string RequestId =>
        httpContextAccessor.HttpContext?.TraceIdentifier
        ?? Guid.CreateVersion7().ToString("n");

    public string? TraceId =>
        System.Diagnostics.Activity.Current?.TraceId.ToString();

    public Guid? UserId => currentUserService.UserId;

    public Guid? TenantId => tenantContext.TenantId;

    public string? ClientIp =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

    public DateTimeOffset RequestStartTime { get; } = DateTimeOffset.UtcNow;
}
