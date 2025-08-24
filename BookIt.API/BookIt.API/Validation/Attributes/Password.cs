using BookIt.API.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation.Attributes;

public class PasswordValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password)
            return ValidationResult.Success;

        var errors = new List<string>();

        if (!password.Any(char.IsUpper))
            errors.Add("contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            errors.Add("contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            errors.Add("contain at least one number");

        var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        if (!password.Any(specialChars.Contains))
            errors.Add("contain at least one special character");

        for (int i = 0; i < password.Length - 2; i++)
        {
            if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
            {
                errors.Add("contain no more than 2 consecutive identical characters");
                break;
            }
        }

        if (validationContext.ObjectInstance is RegisterRequest request)
        {
            var username = request.Username?.ToLower();
            if (!string.IsNullOrEmpty(username) && password.ToLower().Contains(username))
                errors.Add("not contain the username");
        }

        if (errors.Any())
            return new ValidationResult($"Password must {string.Join(", ", errors)}");

        return ValidationResult.Success;
    }
}
