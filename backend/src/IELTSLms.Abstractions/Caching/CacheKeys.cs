namespace IELTSLms.Abstractions.Caching;

/// <summary>
/// Namespace-based cache key builder. Produces deterministic, collision-free keys of the form
/// <c>namespace:entity:id</c> and supports tenant/user scoping so cached data never leaks
/// across security boundaries (Security Standards §7, Backend Architecture §11).
/// </summary>
public static class CacheKeys
{
    private const char Separator = ':';

    /// <summary>Builds a key: <c>{namespace}:{key}</c>.</summary>
    public static string Create(string ns, string key) =>
        string.Concat(ns, Separator, key);

    /// <summary>Builds a key: <c>{namespace}:{subNamespace}:{key}</c>.</summary>
    public static string Create(string ns, string subNamespace, string key) =>
        string.Concat(ns, Separator, subNamespace, Separator, key);

    /// <summary>Builds a tenant-scoped key: <c>t:{tenantId}:{namespace}:{key}</c>.</summary>
    public static string ForTenant(Guid tenantId, string ns, string key) =>
        string.Concat("t:", tenantId.ToString("N"), Separator, ns, Separator, key);

    /// <summary>Builds a user-scoped key: <c>u:{userId}:{namespace}:{key}</c>.</summary>
    public static string ForUser(Guid userId, string ns, string key) =>
        string.Concat("u:", userId.ToString("N"), Separator, ns, Separator, key);

    /// <summary>Builds a tenant+user-scoped key: <c>t:{tenantId}:u:{userId}:{namespace}:{key}</c>.</summary>
    public static string ForTenantUser(Guid tenantId, Guid userId, string ns, string key) =>
        string.Concat("t:", tenantId.ToString("N"), ":u:", userId.ToString("N"), Separator, ns, Separator, key);

    /// <summary>Returns the prefix for all tenant-scoped keys: <c>t:{tenantId}:</c>.</summary>
    public static string TenantPrefix(Guid tenantId) =>
        string.Concat("t:", tenantId.ToString("N"), ":");

    /// <summary>Returns the prefix for all user-scoped keys: <c>u:{userId}:</c>.</summary>
    public static string UserPrefix(Guid userId) =>
        string.Concat("u:", userId.ToString("N"), ":");
}
