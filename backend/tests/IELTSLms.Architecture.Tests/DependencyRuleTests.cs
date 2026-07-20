using NetArchTest.Rules;

namespace IELTSLms.Architecture.Tests;

/// <summary>
/// Enforces the layer reference rules from Backend Architecture §5. These fail the build on any
/// violation of the golden rule: nothing references <c>Api</c>; only <c>Api</c> references
/// <c>Infrastructure</c>/<c>Persistence</c>; <c>Domain</c> references only <c>SharedKernel</c>.
/// </summary>
public sealed class DependencyRuleTests
{
    [Fact]
    public void Domain_should_not_depend_on_any_other_layer()
    {
        var result = Types.InAssembly(Layers.Domain)
            .Should()
            .NotHaveDependencyOnAny(
                Layers.ApplicationNs,
                Layers.InfrastructureNs,
                Layers.PersistenceNs,
                Layers.ApiNs,
                Layers.BackgroundJobsNs,
                "IELTSLms.Abstractions",
                "IELTSLms.Contracts",
                "IELTSLms.Common")
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Domain_should_not_depend_on_frameworks()
    {
        var result = Types.InAssembly(Layers.Domain)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "MediatR",
                "Npgsql")
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_persistence_or_api()
    {
        var result = Types.InAssembly(Layers.Application)
            .Should()
            .NotHaveDependencyOnAny(
                Layers.InfrastructureNs,
                Layers.PersistenceNs,
                Layers.ApiNs)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Application_should_not_depend_on_ef_core_or_providers()
    {
        var result = Types.InAssembly(Layers.Application)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "StackExchange.Redis")
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Abstractions_should_not_depend_on_infrastructure_persistence_or_api()
    {
        var result = Types.InAssembly(Layers.Abstractions)
            .Should()
            .NotHaveDependencyOnAny(
                Layers.ApplicationNs,
                Layers.InfrastructureNs,
                Layers.PersistenceNs,
                Layers.ApiNs)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(Layers.Infrastructure)
            .Should()
            .NotHaveDependencyOn(Layers.ApiNs)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void Persistence_should_not_depend_on_api_or_infrastructure()
    {
        var result = Types.InAssembly(Layers.Persistence)
            .Should()
            .NotHaveDependencyOnAny(Layers.ApiNs, Layers.InfrastructureNs)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void BackgroundJobs_should_not_depend_on_infrastructure_persistence_or_api()
    {
        var result = Types.InAssembly(Layers.BackgroundJobs)
            .Should()
            .NotHaveDependencyOnAny(
                Layers.InfrastructureNs,
                Layers.PersistenceNs,
                Layers.ApiNs)
            .GetResult();

        AssertNoViolations(result);
    }

    [Fact]
    public void SharedKernel_should_not_depend_on_any_other_layer()
    {
        var result = Types.InAssembly(Layers.SharedKernel)
            .Should()
            .NotHaveDependencyOnAny(
                Layers.DomainNs,
                Layers.ApplicationNs,
                Layers.InfrastructureNs,
                Layers.PersistenceNs,
                Layers.ApiNs,
                "IELTSLms.Abstractions",
                "IELTSLms.Contracts",
                "IELTSLms.Common")
            .GetResult();

        AssertNoViolations(result);
    }

    private static void AssertNoViolations(TestResult result)
    {
        Assert.True(
            result.IsSuccessful,
            result.IsSuccessful
                ? string.Empty
                : "Architecture rule violated by: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}
