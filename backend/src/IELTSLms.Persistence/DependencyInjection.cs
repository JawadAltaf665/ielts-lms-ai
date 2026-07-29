using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Persistence.Audit;
using IELTSLms.Persistence.Interceptors;
using IELTSLms.Persistence.Repositories;
using IELTSLms.Persistence.Repositories.Billing;
using IELTSLms.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.Persistence;

/// <summary>
/// Composition helpers for the persistence layer: registers the EF Core context against PostgreSQL
/// and exposes it as the application's <see cref="IUnitOfWork"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the primary PostgreSQL connection string.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>Registers persistence-layer services into the container.</summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found in configuration.");

        // Register interceptors as scoped services (they depend on scoped context services)
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditLogInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure();
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Audit infrastructure
        services.AddScoped<IAuditLogWriter, EfAuditLogWriter>();
        services.AddScoped<IActivityLogger, EfActivityLogger>();

        AddRepositories(services);

        services.AddHostedService<SystemTenantSeeder>();

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
        
        // Phase 4 - Identity & Tenancy
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IKnownDeviceRepository, KnownDeviceRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        // Phase 5 - Subscription & Billing
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IPriceRepository, PriceRepository>();
        services.AddScoped<IFeatureGroupRepository, FeatureGroupRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IPlanFeatureRepository, PlanFeatureRepository>();
        services.AddScoped<ITenantFeatureRepository, TenantFeatureRepository>();
        services.AddScoped<IQuotaDefinitionRepository, QuotaDefinitionRepository>();
        services.AddScoped<IPlanQuotaRepository, PlanQuotaRepository>();
        services.AddScoped<ITenantQuotaRepository, TenantQuotaRepository>();
        services.AddScoped<IUsageRecordRepository, UsageRecordRepository>();
        services.AddScoped<IBillingCustomerRepository, BillingCustomerRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionItemRepository, SubscriptionItemRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceLineRepository, InvoiceLineRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProcessedWebhookEventRepository, ProcessedWebhookEventRepository>();

        // Phase 5 Sprint 2 - Billing Operations (ADR-032)
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponRedemptionRepository, CouponRedemptionRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();
        services.AddScoped<IFailedWebhookEventRepository, FailedWebhookEventRepository>();
    }
}
