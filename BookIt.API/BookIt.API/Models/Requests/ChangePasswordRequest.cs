using BookIt.API.Validation.Attributes;
using BookIt.DAL.Constants;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Models.Requests;

public record ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required")]
    [StringLength(UserConstants.MaxPasswordLength, MinimumLength = UserConstants.MinPasswordLength, ErrorMessage = "Current password must be between 8 and 128 characters")]
    [PasswordValidation]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(UserConstants.MaxPasswordLength, MinimumLength = UserConstants.MinPasswordLength, ErrorMessage = "New password must be between 8 and 128 characters")]
    [PasswordValidation]
    public string NewPassword { get; init; } = string.Empty;
}
