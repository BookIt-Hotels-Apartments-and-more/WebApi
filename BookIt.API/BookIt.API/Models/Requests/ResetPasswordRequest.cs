namespace BookIt.API.Models.Requests;

public record ResetPasswordRequest
{
    public string NewPassword { get; set; } = null!;
    public string Token { get; set; } = null!;
}