using IELTSLms.Abstractions.Environment;
using Microsoft.Extensions.Hosting;

namespace IELTSLms.Infrastructure.Environment;

internal sealed class EnvironmentService(IHostEnvironment hostEnvironment) : IEnvironmentService
{
    public string EnvironmentName => hostEnvironment.EnvironmentName;
    public bool IsProduction => hostEnvironment.IsProduction();
    public bool IsDevelopment => hostEnvironment.IsDevelopment();
}
