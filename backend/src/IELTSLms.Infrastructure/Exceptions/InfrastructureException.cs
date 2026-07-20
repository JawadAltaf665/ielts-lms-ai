namespace IELTSLms.Infrastructure.Exceptions;

public sealed class InfrastructureException : Exception
{
    public InfrastructureException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
