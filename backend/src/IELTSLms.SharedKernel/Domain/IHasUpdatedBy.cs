namespace IELTSLms.SharedKernel.Domain;

public interface IHasUpdatedBy
{
    Guid? UpdatedBy { get; }
}
