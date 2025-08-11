namespace BookIt.API.Models.Responses;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public Dictionary<string, object>? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
