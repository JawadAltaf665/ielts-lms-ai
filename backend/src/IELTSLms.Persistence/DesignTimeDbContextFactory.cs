using IELTSLms.Abstractions.Events;
using IELTSLms.Abstractions.Tenancy;
using IELTSLms.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IELTSLms.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef migrations</c>/<c>database update</c> can construct the
/// context without booting the API host. The connection string here is only a placeholder used when
/// scaffolding migrations (which needs the model, not a live connection); to apply migrations against
/// a real database, pass <c>--connection "&lt;connection string&gt;"</c>, which EF applies over this
/// value. Domain-event dispatch is stubbed because it never runs at design time.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=ielts_lms;Username=postgres;Password=Postgres123")
            .Options;

        return new ApplicationDbContext(options, new NoOpDomainEventDispatcher(), new NoOpTenantContext());
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    /// <summary>
    /// Design-time tenant context: no tenant is resolved and the filter is treated as bypassed, so the
    /// model builds without an ambient request. Migrations describe schema, not data, so this never
    /// affects a real query.
    /// </summary>
    private sealed class NoOpTenantContext : ITenantContext
    {
        public Guid? TenantId => null;

        public bool HasTenant => false;

        public bool IsBypassed => true;
    }
}
