namespace IELTSLms.SharedKernel.Domain;

public interface IRestorable : ISoftDeletable
{
    DateTimeOffset? RestoredAtUtc { get; }
    Guid? RestoredBy { get; }
}
