namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Marker for domain events that fire after the transaction commits (the current default behavior).
/// </summary>
public interface IPostCommitDomainEvent : IDomainEvent;
