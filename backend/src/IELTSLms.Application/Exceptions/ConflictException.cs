namespace IELTSLms.Application.Exceptions;

public sealed class ConflictException : Exception
{
    public ConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
