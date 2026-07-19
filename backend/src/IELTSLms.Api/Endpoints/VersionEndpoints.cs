using System.Reflection;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Read-only endpoint reporting the running API version and environment. Useful for smoke tests
/// and deploy verification; carries no tenant data and is safe to expose.
/// </summary>
internal static class VersionEndpoints
{
    public static IEndpointRouteBuilder MapVersionEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/version", (IHostEnvironment environment) =>
        {
            var informationalVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            return Results.Ok(new
            {
                service = "IELTS LMS AI API",
                version = informationalVersion ?? "1.0.0",
                environment = environment.EnvironmentName,
            });
        })
        .WithName("GetVersion")
        .WithSummary("Returns the running API version and environment.")
        .WithTags("Diagnostics");

        return builder;
    }
}
