namespace BookIt.API.Models.Requests;

public record GenerateResetPasswordTokenRequest
{
    public string Email { get; set; } = null!;
}