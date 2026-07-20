using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.Infrastructure.Performance;

/// <summary>
/// Configures response compression (Brotli + Gzip), request size limits, and other
/// performance-related middleware settings (Infrastructure §4, Security §12).
/// </summary>
public static class PerformanceRegistration
{
    public static IServiceCollection AddPerformanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(PerformanceOptions.SectionName)
            .Get<PerformanceOptions>() ?? new PerformanceOptions();

        if (options.EnableResponseCompression)
        {
            services.AddResponseCompression(o =>
            {
                o.EnableForHttps = false; // Prevent BREACH-type attacks on HTTPS
                o.Providers.Add<BrotliCompressionProvider>();
                o.Providers.Add<GzipCompressionProvider>();
                o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                [
                    "application/json",
                    "application/problem+json",
                ]);
            });

            services.Configure<BrotliCompressionProviderOptions>(o =>
                o.Level = CompressionLevel.Fastest);
            services.Configure<GzipCompressionProviderOptions>(o =>
                o.Level = CompressionLevel.SmallestSize);
        }

        services.AddRequestTimeouts(o =>
        {
            o.DefaultPolicy = new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds),
            };
        });

        return services;
    }
}
