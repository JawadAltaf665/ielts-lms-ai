namespace IELTSLms.SharedKernel.Exceptions;

public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message)
        : base(code, message)
    {
    }
}
