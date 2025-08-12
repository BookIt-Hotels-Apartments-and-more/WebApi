using BookIt.API.Validation;
using BookIt.DAL.Constants;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record UserDetailsRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(UserConstants.MaxUsernameLength, MinimumLength = UserConstants.MinUsernameLength, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[ a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, spaces, dots, and hyphens")]
    [UsernameValidation]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string? Bio { get; set; } = null;
}
