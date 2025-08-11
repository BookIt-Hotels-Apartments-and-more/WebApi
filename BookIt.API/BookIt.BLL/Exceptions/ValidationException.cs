namespace BookIt.BLL.Exceptions;

public class ValidationException : BookItBaseException
{
    public Dictionary<string, List<string>> Errors { get; }

    public ValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, List<string>>();
    }

    public ValidationException(Dictionary<string, List<string>> errors)
        : base("One or more validation errors occurred", "VALIDATION_ERROR")
    {
        Errors = errors;
        Properties["ValidationErrors"] = errors;
    }

    public ValidationException(string field, string error)
        : base($"Validation failed for {field}: {error}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, List<string>>
        {
            { field, new List<string> { error } }
        };
        Properties["ValidationErrors"] = Errors;
    }
}