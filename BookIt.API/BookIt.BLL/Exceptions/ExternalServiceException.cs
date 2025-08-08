namespace BookIt.BLL.Exceptions;

public class ExternalServiceException : BookItBaseException
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public ExternalServiceException(string serviceName, string message, int? statusCode = null)
        : base($"External service '{serviceName}' error: {message}", "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
        Properties["ServiceName"] = serviceName;
        if (statusCode.HasValue) Properties["StatusCode"] = statusCode;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException, int? statusCode = null)
        : base($"External service '{serviceName}' error: {message}", "EXTERNAL_SERVICE_ERROR", innerException)
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
        Properties["ServiceName"] = serviceName;
        if (statusCode.HasValue) Properties["StatusCode"] = statusCode;
    }
}