namespace IELTSLms.Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        Code = "NOT_FOUND";
        EntityName = entityName;
        EntityId = entityId;
    }

    public string Code { get; }
    public string EntityName { get; }
    public object EntityId { get; }
}
