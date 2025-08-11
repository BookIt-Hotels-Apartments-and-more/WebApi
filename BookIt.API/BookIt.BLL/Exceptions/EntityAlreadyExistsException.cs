namespace BookIt.BLL.Exceptions;

public class EntityAlreadyExistsException : BookItBaseException
{
    public string EntityType { get; }
    public string Field { get; }
    public object Value { get; }

    public EntityAlreadyExistsException(string entityType, string field, object value)
        : base($"{entityType} with {field} '{value}' already exists", "ENTITY_ALREADY_EXISTS")
    {
        EntityType = entityType;
        Field = field;
        Value = value;
        Properties["EntityType"] = entityType;
        Properties["Field"] = field;
        Properties["Value"] = value;
    }
}