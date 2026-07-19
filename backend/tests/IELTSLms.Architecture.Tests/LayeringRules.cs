using System.Reflection;
using NetArchTest.Rules;

namespace IELTSLms.Architecture.Tests;

/// <summary>
/// Central registry of the layer assemblies and their namespaces, so the rule tests read
/// declaratively. Each entry is anchored to a real type in that assembly to force it to load.
/// </summary>
internal static class Layers
{
    public static readonly Assembly SharedKernel = typeof(SharedKernel.Results.Result).Assembly;
    public static readonly Assembly Domain = typeof(Domain.AssemblyMarker).Assembly;
    public static readonly Assembly Abstractions = typeof(Abstractions.Time.IClock).Assembly;
    public static readonly Assembly Contracts = typeof(Contracts.AssemblyMarker).Assembly;
    public static readonly Assembly Common = typeof(Common.AssemblyMarker).Assembly;
    public static readonly Assembly Application = typeof(Application.DependencyInjection).Assembly;
    public static readonly Assembly Infrastructure = typeof(Infrastructure.DependencyInjection).Assembly;
    public static readonly Assembly Persistence = typeof(Persistence.DependencyInjection).Assembly;
    public static readonly Assembly BackgroundJobs = typeof(BackgroundJobs.DependencyInjection).Assembly;

    public const string DomainNs = "IELTSLms.Domain";
    public const string ApplicationNs = "IELTSLms.Application";
    public const string InfrastructureNs = "IELTSLms.Infrastructure";
    public const string PersistenceNs = "IELTSLms.Persistence";
    public const string ApiNs = "IELTSLms.Api";
    public const string BackgroundJobsNs = "IELTSLms.BackgroundJobs";
}
