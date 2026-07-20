namespace IELTSLms.SharedKernel.Exceptions;

/// <summary>
/// Base for domain-layer exceptions that represent invariant violations which cannot
/// be expressed via the Result pattern (e.g., programming errors surfacing in production).
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
