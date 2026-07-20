namespace IELTSLms.Application.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string entityName, object? entityId = null)
        : base($"A concurrency conflict occurred on {entityName}{(entityId is not null ? $" '{entityId}'" : "")}. The resource was modified by another request.")
    {
        Code = "CONFLICT_VERSION_MISMATCH";
        EntityName = entityName;
        EntityId = entityId;
    }

    public string Code { get; }
    public string EntityName { get; }
    public object? EntityId { get; }
}
