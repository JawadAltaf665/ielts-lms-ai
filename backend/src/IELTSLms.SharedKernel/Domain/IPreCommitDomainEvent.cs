namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marker for domain events that fire before the transaction commits, allowing handlers to
/// modify tracked entities within the same transaction.
/// </summary>
public interface IPreCommitDomainEvent : IDomainEvent;
