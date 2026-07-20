namespace IELTSLms.Abstractions.Context;

public interface ICorrelationIdProvider
{
    string CorrelationId { get; }
}
