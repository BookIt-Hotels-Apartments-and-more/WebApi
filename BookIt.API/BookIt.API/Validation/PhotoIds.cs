using System.ComponentModel.DataAnnotations;

namespace BookIt.API.Validation;

public class PhotoIdsValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<int> photoIds)
            return ValidationResult.Success;

        var invalidIds = photoIds.Where(id => id < 1).ToList();

        if (invalidIds.Any())
            return new ValidationResult($"All photo IDs must be positive numbers. Invalid IDs: {string.Join(", ", invalidIds)}");

        var duplicates = photoIds.GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
            return new ValidationResult($"Duplicate photo IDs are not allowed: {string.Join(", ", duplicates)}");

        return ValidationResult.Success;
    }
}
