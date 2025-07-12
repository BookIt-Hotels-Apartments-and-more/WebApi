using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation;

public class UsernameValidationAttribute : ValidationAttribute
{
    private static readonly string[] ReservedUsernames =
    {
        "admin", "administrator", "root", "system", "api", "www", "mail", "ftp",
        "support", "help", "info", "contact", "service", "guest", "user", "test",
        "demo", "null", "undefined", "anonymous", "public", "private"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string username)
            return ValidationResult.Success;

        if (ReservedUsernames.Contains(username.ToLower()))
            return new ValidationResult($"Username '{username}' cannot be used");

        if (username.StartsWith('.') || username.StartsWith('-') || username.StartsWith('_') ||
            username.EndsWith('.') || username.EndsWith('-') || username.EndsWith('_'))
            return new ValidationResult("Username cannot start or end with dots, hyphens, or underscores");

        if (username.Contains("..") || username.Contains("--") || username.Contains("__") ||
            username.Contains(".-") || username.Contains("-.") || username.Contains("_-") ||
            username.Contains("-_") || username.Contains("._") || username.Contains("_."))
            return new ValidationResult("Username cannot contain consecutive special characters");

        return ValidationResult.Success;
    }
}
