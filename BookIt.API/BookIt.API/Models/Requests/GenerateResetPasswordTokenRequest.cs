using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record GenerateResetPasswordTokenRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string Email { get; set; } = null!;
}