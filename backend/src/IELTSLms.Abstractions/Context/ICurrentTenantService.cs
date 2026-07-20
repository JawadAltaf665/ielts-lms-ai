namespace IELTSLms.Abstractions.Context;

/// <summary>
/// Provides the resolved tenant for the current request scope.
/// </summary>
public interface ICurrentTenantService
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
}
