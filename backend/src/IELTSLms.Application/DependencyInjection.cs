using System.Reflection;
using FluentValidation;
using IELTSLms.Abstractions.Billing;
using IELTSLms.Application.Abstractions.Behaviors;
using IELTSLms.Application.Billing.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.Application;

/// <summary>
/// Composition helpers for the application layer. Registers MediatR handlers, FluentValidation
/// validators, and the cross-cutting pipeline behaviors in the correct order.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Registers application-layer services into the container.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);

            // Pipeline execution order (outermost → innermost):
            configuration.AddOpenBehavior(typeof(CancellationPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(CorrelationPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(PerformancePipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(ExceptionPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(AuthorizationPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(AuditPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddScoped<IEntitlementService, EntitlementService>();
        services.AddScoped<IQuotaService, QuotaService>();

        return services;
    }
}
