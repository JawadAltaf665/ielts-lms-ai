namespace IELTSLms.Abstractions.Environment;

public interface IEnvironmentService
{
    string EnvironmentName { get; }
    bool IsProduction { get; }
    bool IsDevelopment { get; }
}
