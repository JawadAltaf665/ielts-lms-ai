using System.Reflection;
using FluentValidation;
using IELTSLms.Application.Abstractions.Behaviors;
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

            // Order matters: logging wraps the whole pipeline, validation runs before the handler.
            configuration.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
