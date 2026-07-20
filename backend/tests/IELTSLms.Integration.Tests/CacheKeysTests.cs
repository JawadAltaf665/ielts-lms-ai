using IELTSLms.Abstractions.Caching;

namespace IELTSLms.Integration.Tests;

/// <summary>
/// Verifies CacheKeys helper produces correctly namespaced, collision-free cache keys
/// (unit-level, no external dependencies).
/// </summary>
public sealed class CacheKeysTests
{
    [Fact]
    public void Create_with_namespace_and_key()
    {
        var result = CacheKeys.Create("users", "profile-123");

        Assert.Equal("users:profile-123", result);
    }

    [Fact]
    public void Create_with_sub_namespace()
    {
        var result = CacheKeys.Create("users", "settings", "dark-mode");

        Assert.Equal("users:settings:dark-mode", result);
    }

    [Fact]
    public void ForTenant_includes_tenant_prefix()
    {
        var tenantId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var result = CacheKeys.ForTenant(tenantId, "courses", "list");

        Assert.StartsWith("t:11111111222233334444555555555555:", result);
        Assert.Contains("courses:list", result);
    }

    [Fact]
    public void ForUser_includes_user_prefix()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var result = CacheKeys.ForUser(userId, "prefs", "theme");

        Assert.StartsWith("u:aaaaaaaabbbbccccddddeeeeeeeeeeee:", result);
        Assert.Contains("prefs:theme", result);
    }

    [Fact]
    public void ForTenantUser_includes_both_scopes()
    {
        var tenantId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var result = CacheKeys.ForTenantUser(tenantId, userId, "dashboard", "widgets");

        Assert.Contains("t:11111111222233334444555555555555:", result);
        Assert.Contains("u:aaaaaaaabbbbccccddddeeeeeeeeeeee:", result);
        Assert.Contains("dashboard:widgets", result);
    }

    [Fact]
    public void TenantPrefix_is_deterministic()
    {
        var tenantId = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var a = CacheKeys.TenantPrefix(tenantId);
        var b = CacheKeys.TenantPrefix(tenantId);

        Assert.Equal(a, b);
        Assert.Equal("t:11111111222233334444555555555555:", a);
    }

    [Fact]
    public void Different_tenants_produce_different_keys()
    {
        var t1 = CacheKeys.ForTenant(Guid.NewGuid(), "ns", "key");
        var t2 = CacheKeys.ForTenant(Guid.NewGuid(), "ns", "key");

        Assert.NotEqual(t1, t2);
    }
}
