namespace BookIt.BLL.Exceptions;

public class UnauthorizedOperationException : BookItBaseException
{
    public int? UserId { get; }
    public string Operation { get; }

    public UnauthorizedOperationException(string operation, int? userId = null)
        : base($"User is not authorized to perform operation: {operation}", "UNAUTHORIZED_OPERATION")
    {
        Operation = operation;
        UserId = userId;
        Properties["Operation"] = operation;
        if (userId.HasValue) Properties["UserId"] = userId;
    }
}