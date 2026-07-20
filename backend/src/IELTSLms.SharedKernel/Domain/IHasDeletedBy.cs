namespace IELTSLms.SharedKernel.Domain;

public interface IHasDeletedBy
{
    Guid? DeletedBy { get; }
}
