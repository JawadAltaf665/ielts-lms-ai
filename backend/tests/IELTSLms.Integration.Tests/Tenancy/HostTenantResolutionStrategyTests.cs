using IELTSLms.Api.Tenancy;
using IELTSLms.Domain.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IELTSLms.Integration.Tests.Tenancy;

/// <summary>
/// Unit-style tests for <see cref="HostTenantResolutionStrategy"/> — mocked <see cref="FakeTenantRepository"/>,
/// no database. Lives here (not Application.Tests) because the Api project may only be referenced by
/// Integration.Tests (Architecture.Tests §"nothing references Api").
/// </summary>
public sealed class HostTenantResolutionStrategyTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static Tenant CreateTenant(string slug, string code, string? primaryDomain = null) =>
        Tenant.Create(
            TenantId.From(Guid.NewGuid()),
            TenantSlug.Create(slug).Value,
            TenantCode.Create(code).Value,
            name: slug,
            Now,
            primaryDomain: primaryDomain);

    private static HostTenantResolutionStrategy CreateStrategy(
        FakeTenantRepository tenants, params string[] rootDomains) =>
        new(tenants, Options.Create(new TenantResolutionOptions { RootDomains = rootDomains }));

    private static DefaultHttpContext ContextWithHost(string host) =>
        new() { Request = { Host = new HostString(host) } };

    [Fact]
    public async Task Resolves_a_subdomain_of_a_configured_root_domain_to_its_tenant()
    {
        var tenants = new FakeTenantRepository();
        var acme = CreateTenant("acme", "ACME");
        tenants.Seed(acme);
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("acme.ielts-lms.ai"));

        Assert.Equal(acme.Id.Value, resolved);
    }

    [Fact]
    public async Task Is_case_insensitive_on_the_host()
    {
        var tenants = new FakeTenantRepository();
        var acme = CreateTenant("acme", "ACME");
        tenants.Seed(acme);
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("ACME.IELTS-LMS.AI"));

        Assert.Equal(acme.Id.Value, resolved);
    }

    [Fact]
    public async Task Resolves_nothing_for_the_bare_apex_root_domain()
    {
        var tenants = new FakeTenantRepository();
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("ielts-lms.ai"));

        Assert.Null(resolved);
    }

    [Fact]
    public async Task Resolves_nothing_for_an_unknown_subdomain()
    {
        var tenants = new FakeTenantRepository();
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("nobody.ielts-lms.ai"));

        Assert.Null(resolved);
    }

    [Fact]
    public async Task Resolves_nothing_for_a_host_matching_no_configured_root_domain()
    {
        var tenants = new FakeTenantRepository();
        tenants.Seed(CreateTenant("acme", "ACME"));
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("localhost"));

        Assert.Null(resolved);
    }

    [Fact]
    public async Task Falls_back_to_a_tenants_custom_primary_domain_when_the_host_matches_no_root_domain()
    {
        var tenants = new FakeTenantRepository();
        var branded = CreateTenant("branded", "BRANDED", primaryDomain: "learn.branded-institute.com");
        tenants.Seed(branded);
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("learn.branded-institute.com"));

        Assert.Equal(branded.Id.Value, resolved);
    }

    [Fact]
    public async Task Ignores_the_port_on_the_host_header()
    {
        var tenants = new FakeTenantRepository();
        var acme = CreateTenant("acme", "ACME");
        tenants.Seed(acme);
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        var resolved = await strategy.Resolve(ContextWithHost("acme.ielts-lms.ai:5000"));

        Assert.Equal(acme.Id.Value, resolved);
    }

    [Fact]
    public async Task Does_not_treat_a_multi_level_subdomain_as_a_slug()
    {
        var tenants = new FakeTenantRepository();
        tenants.Seed(CreateTenant("acme", "ACME"));
        var strategy = CreateStrategy(tenants, "ielts-lms.ai");

        // "foo.acme" isn't a valid single-label slug even though "acme" is a real tenant.
        var resolved = await strategy.Resolve(ContextWithHost("foo.acme.ielts-lms.ai"));

        Assert.Null(resolved);
    }
}
