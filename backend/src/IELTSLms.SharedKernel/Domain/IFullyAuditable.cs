namespace IELTSLms.SharedKernel.Domain;

/// <summary>
/// Composes all audit concerns: timestamps, actor tracking, soft-delete with actor, and restore.
/// </summary>
public interface IFullyAuditable
    : IAuditableEntity, IHasCreatedBy, IHasUpdatedBy, IHasDeletedBy, IRestorable
{
}
