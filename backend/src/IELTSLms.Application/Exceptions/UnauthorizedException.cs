namespace IELTSLms.Application.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string? message = null)
        : base(message ?? "Authentication is required.")
    {
        Code = "AUTH_UNAUTHENTICATED";
    }

    public string Code { get; }
}
