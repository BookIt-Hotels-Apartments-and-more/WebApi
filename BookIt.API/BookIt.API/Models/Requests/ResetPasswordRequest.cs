using BookIt.API.Validation.Attributes;
using BookIt.DAL.Constants;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record ResetPasswordRequest
{
    [Required(ErrorMessage = "Password is required")]
    [StringLength(UserConstants.MaxPasswordLength, MinimumLength = UserConstants.MinPasswordLength, ErrorMessage = "Password must be between 8 and 128 characters")]
    [PasswordValidation]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Token is required.")]
    public string Token { get; set; } = null!;
}