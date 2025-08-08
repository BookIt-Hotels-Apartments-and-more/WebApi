namespace BookIt.BLL.Exceptions;

public abstract class BookItBaseException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Properties { get; }

    protected BookItBaseException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
        Properties = new Dictionary<string, object>();
    }

    protected BookItBaseException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Properties = new Dictionary<string, object>();
    }
}