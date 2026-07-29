using System.Net;

namespace IELTSLms.Integration.Tests.Billing;

/// <summary>
/// End-to-end checks for the Billing endpoints that don't require a live database: authorization is
/// enforced by middleware before any handler/EF access, so these assert deterministically without a
/// provisioned Postgres instance (18 §12: authorization checks, webhook verification).
/// </summary>
public sealed class BillingEndpointsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory = factory;

    [Fact]
    public async Task GetPlans_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/plans");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSubscription_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/subscription");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CancelSubscription_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/subscription/cancel", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ADR-032 additions (Phase 5 Sprint 2 — Billing Operations).

    [Fact]
    public async Task GetInvoices_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/invoices");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBillingPortalSession_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();
        using var content = new StringContent("{\"returnUrl\":\"https://example.com\"}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/billing-portal", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePlan_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/subscription/change-plan", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PauseSubscription_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/subscription/pause", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemovePaymentMethod_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/v1/payment-methods/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFinancialSummary_without_authentication_returns_unauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reports/financial-summary");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StripeWebhook_without_signature_header_returns_bad_request()
    {
        using var client = _factory.CreateClient();
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/webhooks/stripe", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StripeWebhook_allows_anonymous_access()
    {
        using var client = _factory.CreateClient();
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/v1/webhooks/stripe", content);

        // Reaches the handler (400 for missing signature) rather than 401/403 — confirms AllowAnonymous.
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
