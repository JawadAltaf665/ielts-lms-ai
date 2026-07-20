namespace IELTSLms.SharedKernel.Domain;

public interface IHasCreatedBy
{
    Guid? CreatedBy { get; }
}
