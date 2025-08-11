namespace BookIt.BLL.Exceptions;

public class EntityNotFoundException : BookItBaseException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object id)
        : base($"{entityType} with ID '{id}' was not found", "ENTITY_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = id;
        Properties["EntityType"] = entityType;
        Properties["EntityId"] = id;
    }
}