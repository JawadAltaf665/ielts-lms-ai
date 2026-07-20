namespace IELTSLms.Application.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.")
    {
        Code = "AUTHZ_FORBIDDEN";
    }

    public string Code { get; }
}
