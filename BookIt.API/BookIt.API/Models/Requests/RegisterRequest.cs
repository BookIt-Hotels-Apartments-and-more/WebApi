using BookIt.API.Validation;
using BookIt.DAL.Constants;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(UserConstants.MaxUsernameLength, MinimumLength = UserConstants.MinUsernameLength, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, dots, and hyphens")]
    [UsernameValidation]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(UserConstants.MaxPasswordLength, MinimumLength = UserConstants.MinPasswordLength, ErrorMessage = "Password must be between 8 and 128 characters")]
    [PasswordValidation]
    public string Password { get; set; } = string.Empty;
}
